using EventCentric.EventSourcing;
using EventCentric.Tests.EventSourcing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EventCentric.Tests.EventSourcing
{
    /// <summary>
    /// The performance is really fast. I tried in debug, and it is extremely fast.
    /// </summary>
    [TestClass]
    public class BigMementoPerfTest : GIVEN_store
    {
        [TestMethod]
        [TestCategory(DbIntegrationCategory)]
        public void can_persist_a_memento_of_a_thousand_objects_and_rehydrate_in_memory()
        {
            int namesToRegisterCount = 1000;
            var streamId = Guid.NewGuid();
            var aggregate = new InventoryTestAggregate(streamId);

            IEvent command = null;
            for (int i = 0; i < namesToRegisterCount; i++)
            {
                command = new RegisterItemName($"Item{i}").AsInProcessMessage(Guid.NewGuid(), streamId).AsQueuedEvent(this.appStreamType, Guid.NewGuid(), 1, DateTime.UtcNow, DateTime.Now);
                aggregate.Handle((RegisterItemName)command);
                this.sut.Save(aggregate, command);
                aggregate = this.sut.Get(streamId);
            }
        }

        [TestMethod]
        [TestCategory(DbIntegrationCategory)]
        public void can_persist_a_memento_of_a_thousand_objects_and_then_can_hydrate_from_persisted_memento()
        {
            int namesToRegisterCount = 1000;
            var streamId = Guid.NewGuid();
            var aggregate = new InventoryTestAggregate(streamId);

            IEvent command = null;
            for (int i = 0; i < namesToRegisterCount; i++)
            {
                command = new RegisterItemName($"Item{i}").AsInProcessMessage(Guid.NewGuid(), streamId).AsQueuedEvent(this.appStreamType, Guid.NewGuid(), 1, DateTime.UtcNow, DateTime.Now);
                aggregate.Handle((RegisterItemName)command);
                this.sut.Save(aggregate, command);
                aggregate = this.sut.Get(streamId);
            }

            this.CreateFreshInstanceOfEventStore();
            aggregate = this.sut.Get(streamId);

            Assert.IsTrue(aggregate.Version == namesToRegisterCount);
        }
    }
}
