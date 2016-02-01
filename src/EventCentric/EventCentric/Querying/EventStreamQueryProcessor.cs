using EventCentric.EventSourcing;
using EventCentric.Log;
using EventCentric.Messaging;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Polling;
using EventCentric.Serialization;
using EventCentric.Transport;
using EventCentric.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EventCentric.Querying
{
    public class EventStreamQueryProcessor : Worker,
        IMessageHandler<NewIncomingEvents>
    {
        private readonly string queryName;
        private readonly InMemorySubscriptionRepository subRepo;
        private readonly Poller poller;
        private readonly ConsoleLogger log = new ConsoleLogger();

        private Dictionary<Type, Action<IEvent>> handlers = new Dictionary<Type, Action<IEvent>>();

        public EventStreamQueryProcessor(string queryName = "AnonymousEventStreamQuery")
            : base(new Bus())
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(queryName, nameof(queryName));

            this.queryName = queryName;

            this.subRepo = new InMemorySubscriptionRepository();
            this.poller = new Poller(this.bus, this.log, this.subRepo, new HttpLongPoller(this.bus, this.log, TimeSpan.FromMinutes(2), this.queryName), new JsonTextSerializer(), 1000, 1000);
        }

        public EventStreamQueryProcessor From(params EventSourceConnection[] connections)
        {
            this.subRepo.RegisterSubscriptions(connections);
            return this;
        }

        public EventStreamQueryProcessor When<TEvent>(Action<TEvent> handler)
            where TEvent : IEvent
        {
            this.handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
            return this;
        }

        public EventStreamQueryProcessor AndWhen<TEvent>(Action<TEvent> handler)
            where TEvent : IEvent
            => this.When<TEvent>(handler);

        public void RunUntil(Func<bool> stop)
        {
            this.bus.Send(new StartEventPoller());
            while (true)
            {
                if (stop.Invoke())
                {
                    this.bus.Send(new StopEventPoller());
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Handle(NewIncomingEvents message)
        {
            foreach (var @event in message.IncomingEvents)
            {
                if (this.handlers.ContainsKey(@event.GetType()))
                    this.handlers[@event.GetType()].Invoke(@event);

                this.bus.Publish(new IncomingEventHasBeenProcessed(@event.StreamType, @event.EventCollectionVersion));
            }
        }
    }
}
