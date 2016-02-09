//using EventCentric.EventSourcing;
//using EventCentric.Tests.EventSourcing.Helpers;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Linq;

//namespace EventCentric.Tests.EventSourcing
//{
//    [TestClass]
//    public class WHEN_saving_an_aggregate : GIVEN_store
//    {
//        [TestMethod]
//        [TestCategory(DbIntegrationCategory)]
//        public void THEN_processed_message_and_published_event_are_persisted()
//        {
//            var streamId = Guid.NewGuid();
//            var incomingCommand = new AddItems(5).AsInProcessMessage(Guid.NewGuid(), streamId).AsQueuedEvent(this.appStreamType, Guid.NewGuid(), 1, DateTime.UtcNow, DateTime.Now);
//            var aggregate = new InventoryTestAggregate(streamId);
//            aggregate.Handle((AddItems)incomingCommand);

//            Assert.AreEqual(1, aggregate.PendingEvents.Count());

//            this.sut.Save(aggregate, incomingCommand);

//            var retrievedAggregate = this.sut.Find(streamId);
//            var retrievedMemento = retrievedAggregate.SaveToSnapshot();

//            Assert.IsNotNull(retrievedAggregate);
//            Assert.AreEqual(5, ((InventoryTestAggregateMemento)retrievedMemento).Quantity);
//        }
//    }
//}
