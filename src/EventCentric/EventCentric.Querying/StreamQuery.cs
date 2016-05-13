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
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EventCentric.Querying
{
    public class StreamQuery<TState> : MicroserviceWorker, IGiven<TState>, IWhen<TState>, IRun<TState>,
        IDisposable,
        IMessageHandler<NewIncomingEvents>,
        IMessageHandler<PollResponseWasReceived>,
        IMessageHandler<IncomingEventsHasBeenProcessed>
    {
        private readonly string queryName;
        private readonly InMemorySubscriptionRepository subRepo;
        private readonly List<ProducerPollingStatus> producers = new List<ProducerPollingStatus>();
        private bool producerListIsReady = false;
        private readonly Poller poller;
        private TState state;

        private Dictionary<Type, Func<TState, IEvent, TState>> handlers = new Dictionary<Type, Func<TState, IEvent, TState>>();

        public StreamQuery(string queryName = "AnonymousEventStreamQuery", bool verbose = true)
            : base(new Bus(), new ConsoleLogger(verbose))
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(queryName, nameof(queryName));

            this.queryName = queryName;

            this.subRepo = new InMemorySubscriptionRepository();
            this.poller = new Poller(this.bus, this.log, this.subRepo, new LongPoller(this.bus, this.log, TimeSpan.FromMinutes(2), this.queryName, null), new JsonTextSerializer(), 1000, 1000);
        }

        public IGiven<TState> From(params EventSourceConnection[] connections)
        {
            this.subRepo.RegisterSubscriptions(connections);
            return this;
        }

        IWhen<TState> IGiven<TState>.Given(TState state)
        {
            this.state = state;
            return this;
        }

        IRun<TState> IWhen<TState>.When<TEvent>(Func<TState, TEvent, TState> handler)
        {
            this.handlers.Add(typeof(TEvent), (state, e) => handler(state, (TEvent)e));
            return this;
        }

        IRun<TState> IRun<TState>.And<TEvent>(Func<TState, TEvent, TState> handler)
        {
            this.handlers.Add(typeof(TEvent), (state, e) => handler(state, (TEvent)e));
            return this;
        }

        TState IRun<TState>.RunUntil(Func<bool> stop)
        {
            this.bus.Publish(new StartEventPoller(this.queryName));
            while (!this.stopping)
            {
                if (stop.Invoke())
                    break;
                else
                    Thread.Sleep(100);
            }

            return this.state;
        }

        TState IRun<TState>.Run()
        {
            this.bus.Publish(new StartEventPoller(this.queryName));

            while (!this.stopping)
            {
                if (this.producerListIsReady)
                    break;

                Thread.Sleep(100);
            }

            while (!this.stopping)
            {
                if (this.producers.TrueForAll(p => p.PollCompleted))
                    break;

                Thread.Sleep(100);
            }

            return this.state;
        }

        void IRun<TState>.Run(Action<TState> execute) => execute.Invoke(((IRun<TState>)this).Run());

        TState IRun<TState>.RunAndPrint(Func<TState, string> textFactory)
        {
            var sw = Stopwatch.StartNew();
            ((IRun<TState>)this).Run();
            var elapsedTime = sw.Elapsed;
            Console.WriteLine();
            this.log.Trace($"Query processing has finished. Time elapsed: {elapsedTime.Hours}:{elapsedTime.Minutes}:{elapsedTime.Seconds}:{elapsedTime.Milliseconds}");
            Console.WriteLine();
            Console.WriteLine(textFactory.Invoke(this.state));
            return this.state;
        }

        public void Handle(NewIncomingEvents message)
        {
            foreach (var @event in message.IncomingEvents)
            {
                if (this.handlers.ContainsKey(@event.GetType()))
                {
                    try
                    {
                        this.state = this.handlers[@event.GetType()].Invoke(this.state, @event);
                    }
                    catch (Exception ex)
                    {
                        var wrappedEx = new FatalErrorException($"An error ocurred while handling event of type {@event.GetType()}", ex);
                        this.bus.Publish(new FatalErrorOcurred(wrappedEx));
                        this.log.Error(wrappedEx, "Query failed");
                        throw;
                    }
                }
            }

            this.bus.Publish(new IncomingEventsHasBeenProcessed(message.IncomingEvents.ToArray()));
        }

        public void Handle(PollResponseWasReceived message)
        {
            if (this.producerListIsReady)
                return; // quick return;

            lock (this)
            {
                if (!this.producerListIsReady)
                {
                    if (!this.producers.Any(x => x.ProducerName == message.Response.StreamType))
                        this.producers.Add(new ProducerPollingStatus(message.Response.StreamType, message.Response.ProducerVersion));

                    var subs = this.poller.GetSubscriptionsMetrics().ToList();
                    if (subs.TrueForAll(s => this.producers.Any(p => p.ProducerName == s.ProducerName)))
                    {

                        this.producerListIsReady = true;
                    }
                }
            }
        }

        public void Handle(IncomingEventsHasBeenProcessed message)
        {
            for (int i = 0; i < message.Events.Length; i++)
            {
                var e = message.Events[i];
                var producer = this.producers.Single(x => x.ProducerName == e.StreamType);
                if (producer.MaxVersionToReceive == e.EventCollectionVersion)
                {
                    while (!this.stopping)
                    {
                        if (this.poller.GetBufferPool().Single(x => x.StreamType == producer.ProducerName).CurrentBufferVersion >= producer.MaxVersionToReceive)
                            break;

                        Thread.Sleep(100);
                    }

                    producer.MarkAsCompleted();
                }
            }
        }

        internal class ProducerPollingStatus
        {
            public ProducerPollingStatus(string name, long version)
            {
                this.ProducerName = name;
                this.MaxVersionToReceive = version;
            }

            public string ProducerName { get; }
            public long MaxVersionToReceive { get; }
            public bool PollCompleted { get; private set; } = false;
            internal void MarkAsCompleted() => this.PollCompleted = true;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            this.poller.Handle(new StopEventPoller(true));
        }

        ~StreamQuery()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public interface IGiven<TState>
    {
        IWhen<TState> Given(TState state);
    }

    public interface IWhen<TState>
    {
        IRun<TState> When<TEvent>(Func<TState, TEvent, TState> handler) where TEvent : IEvent;
    }

    public interface IRun<TState>
    {
        IRun<TState> And<TEvent>(Func<TState, TEvent, TState> handler) where TEvent : IEvent;

        TState Run();

        void Run(Action<TState> execute);

        TState RunUntil(Func<bool> stop);

        TState RunAndPrint(Func<TState, string> textFactory);
    }
}
