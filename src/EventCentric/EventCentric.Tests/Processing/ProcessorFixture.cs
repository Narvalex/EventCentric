using EventCentric.Database;
using EventCentric.EventSourcing;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Processing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Tests.Processing.Helpers;
using EventCentric.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Linq;

namespace EventCentric.Tests.Processing.ProcessorFixture
{
    [TestClass]
    public class GIVEN_processor : IDisposable
    {
        protected string connectionString;
        protected TestBus bus;
        protected SubscriptionWriter subWriter;
        protected LocalTimeProvider time = new LocalTimeProvider();
        protected JsonTextSerializer serializer = new JsonTextSerializer();
        protected EventStore<TestAggregate> store;
        protected EventProcessor<TestAggregate> sut;

        public GIVEN_processor()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
            EventStoreDbInitializer.CreateDatabaseObjects(connectionString, true);
            this.bus = new TestBus();
            this.subWriter = new SubscriptionWriter(() => new EventStoreDbContext(this.connectionString), this.time, this.serializer);
            this.store = new EventStore<TestAggregate>(this.serializer, () => new EventStoreDbContext(this.connectionString), this.subWriter, this.time);
            this.sut = new EventProcessor<TestAggregate>(this.bus, this.store, this.subWriter);
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
    }
}
