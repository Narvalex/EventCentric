using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Tests.Processing.Helpers;
using EventCentric.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Tests.Processing.ProcessorFixture
{
    [TestClass]
    public class GIVEN_processor : IDisposable
    {
        protected string connectionString;
        protected TestBus bus;
        protected SubscriptionInboxWriter subWriter;
        protected LocalTimeProvider time = new LocalTimeProvider();
        protected JsonTextSerializer serializer = new JsonTextSerializer();
        protected EventStore<TestAggregate> store;
        protected TestEventProcessor sut;

        public GIVEN_processor()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
            EventStoreWithSubPerStreamDbInitializer.CreateDatabaseObjects(connectionString, true);
            this.bus = new TestBus();
            this.subWriter = new SubscriptionInboxWriter(() => new EventStoreDbContext(this.connectionString), this.time, this.serializer);
            this.store = new EventStore<TestAggregate>(this.serializer, () => new EventStoreDbContext(this.connectionString), this.subWriter, this.time, new SequentialGuid());
            this.sut = new TestEventProcessor(this.bus, this.store, this.subWriter);
        }

        public void Dispose()
        {
            SqlClientLite.DropDatabase(this.connectionString);
        }

        [TestMethod]
        public void THEN_can_start_processor()
        {
            Assert.AreEqual(0, this.bus.Messages.Count);
            this.sut.Handle(new StartEventProcessor());
            Assert.AreEqual(1, this.bus.Messages.Count);
            Assert.AreEqual(typeof(EventProcessorStarted), this.bus.Messages.Single().GetType());
        }

        [TestMethod]
        public void WHEN_multiple_events_arrives_to_update_the_same_stream_THEN_locks_and_handles_graciously()
        {
            using (var context = new EventStoreDbContext(this.connectionString))
            {
                context.Subscriptions.Add(new Repository.Mapping.SubscriptionEntity
                {
                    StreamType = "TestSubscription",
                    CreationDate = DateTime.Now
                });

                context.SaveChanges();
            }

            this.sut.Handle(new StartEventProcessor());

            for (int i = 0; i < 3; i++)
            {
                Task.Factory.StartNewLongRunning(() =>
                    this.sut.Handle(
                        new NewIncomingEvent(
                            new TestQueuedEvent($"Loop number {i}", Guid.Empty, "TestSubscription"))));
            }

            Thread.Sleep(TimeSpan.FromHours(1));
        }
    }
}
