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
    public class EventStreamQueryProcessor : Worker,
        IMessageHandler<NewIncomingEvents>,
        IMessageHandler<PollResponseWasReceived>,
        IMessageHandler<IncomingEventHasBeenProcessed>
    {
        private readonly string queryName;
        private readonly InMemorySubscriptionRepository subRepo;
        private readonly List<ProducerPollingStatus> producers = new List<ProducerPollingStatus>();
        private bool producerListIsReady = false;
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
                    break;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            this.poller.StopSilently();
        }

        public void Run()
        {
            this.bus.Send(new StartEventPoller());

            while (true)
            {
                if (this.producerListIsReady)
                    break;

                Thread.Sleep(100);
            }

            while (true)
            {
                if (this.producers.TrueForAll(p => p.PollCompleted))
                    break;

                Thread.Sleep(100);
            }

            this.poller.StopSilently();
        }

        public void RunAndPrint(Func<string> textFactory)
        {
            var sw = Stopwatch.StartNew();
            this.Run();
            var elapsedTime = sw.Elapsed;
            Console.WriteLine();
            this.log.Trace($"Query processing has finished. Time elapsed: {elapsedTime.Hours}:{elapsedTime.Minutes}:{elapsedTime.Seconds}:{elapsedTime.Milliseconds}");
            Console.WriteLine();
            Console.WriteLine(textFactory.Invoke());
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

        public void Handle(IncomingEventHasBeenProcessed message)
        {
            var producer = this.producers.Single(x => x.ProducerName == message.StreamType);
            if (producer.MaxVersionToReceive == message.EventCollectionVersion)
            {
                while (true)
                {
                    if (this.poller.GetBufferPool().Single(x => x.StreamType == producer.ProducerName).CurrentBufferVersion >= producer.MaxVersionToReceive)
                        break;

                    Thread.Sleep(100);
                }

                producer.MarkAsCompleted();
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
    }
}
