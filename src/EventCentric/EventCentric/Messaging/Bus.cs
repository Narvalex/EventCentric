using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace EventCentric.Messaging
{
    public class Bus : IBus, IBusRegistry
    {
        private readonly Dictionary<Type, List<Tuple<Type, Action<Envelope>>>> handlersByMessageType;
        private readonly Dictionary<Type, Action<IMessage>> dispatchersByMessageType;

        public int HandlersByMessageTypeCount { get { return this.handlersByMessageType.Count; } }
        public int DispatchersByMessageTypeCount { get { return this.dispatchersByMessageType.Count; } }

        public Bus()
        {
            this.handlersByMessageType = new Dictionary<Type, List<Tuple<Type, Action<Envelope>>>>();
            this.dispatchersByMessageType = new Dictionary<Type, Action<IMessage>>();
        }

        public void Publish(IMessage message)
        {
            Action<IMessage> dispatch;

            if (this.dispatchersByMessageType.TryGetValue(message.GetType(), out dispatch))
                dispatch(message);
        }

        public void Register(IWorker worker)
        {
            var handlerType = worker.GetType();

            foreach (var invocationTuple in this.BuildHandlerInvocations(worker))
            {
                var envelopeType = typeof(Envelope<>).MakeGenericType(invocationTuple.Item1);

                List<Tuple<Type, Action<Envelope>>> invocations;
                if (!this.handlersByMessageType.TryGetValue(invocationTuple.Item1, out invocations))
                {
                    invocations = new List<Tuple<Type, Action<Envelope>>>();
                    this.handlersByMessageType[invocationTuple.Item1] = invocations;
                }

                invocations.Add(new Tuple<Type, Action<Envelope>>(handlerType, invocationTuple.Item2));

                if (!this.dispatchersByMessageType.ContainsKey(invocationTuple.Item1))
                    this.dispatchersByMessageType[invocationTuple.Item1] = this.BuildDispatchInvocation(invocationTuple.Item1);
            }
        }

        private IEnumerable<Tuple<Type, Action<Envelope>>> BuildHandlerInvocations(IWorker worker)
        {
            var interfaces = worker.GetType().GetInterfaces();

            var messageHandlerInvocations =
                interfaces
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
                .Select(i => new { HandlerInterface = i, MessageType = i.GetGenericArguments()[0] })
                .Select(m => new Tuple<Type, Action<Envelope>>(m.MessageType, this.BuildHandlerInvocation(worker, m.HandlerInterface, m.MessageType)));

            return messageHandlerInvocations;
        }

        private Action<Envelope> BuildHandlerInvocation(IWorker worker, Type handlerType, Type messageType)
        {
            var envelopeType = typeof(Envelope<>).MakeGenericType(messageType);
            var parameter = Expression.Parameter(typeof(Envelope));

            var invocationExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Convert(Expression.Constant(worker), handlerType),
                            handlerType.GetMethod("Handle"),
                            Expression.Property(Expression.Convert(parameter, envelopeType), "Body"))),
                parameter);

            return (Action<Envelope>)invocationExpression.Compile();
        }

        private Action<IMessage> BuildDispatchInvocation(Type messageType)
        {
            var messageParameter = Expression.Parameter(typeof(IMessage));

            var dispatchExpression =
                Expression.Lambda(
                    Expression.Block(
                        Expression.Call(
                            Expression.Constant(this),
                            this.GetType().GetMethod("DoDispatchMessage", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(messageType),
                            Expression.Convert(messageParameter, messageType))),
                    messageParameter);

            return (Action<IMessage>)dispatchExpression.Compile();
        }

        private void DoDispatchMessage<T>(T message) where T : IMessage
        {
            var envelope = Envelope.Create(message);

            List<Tuple<Type, Action<Envelope>>> handlers;
            if (this.handlersByMessageType.TryGetValue(typeof(T), out handlers))
                foreach (var handler in handlers)
                    Task.Factory.StartNewLongRunning(() => handler.Item2(envelope));
        }

        public void Register(params IWorker[] workers)
        {
            foreach (var worker in workers)
                this.Register(worker);
        }

        public void Publish(params IMessage[] messages)
        {
            foreach (var message in messages)
                this.Publish(message);
        }
    }
}
