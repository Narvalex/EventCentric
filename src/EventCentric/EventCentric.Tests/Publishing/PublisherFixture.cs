using EventCentric.Log;
using EventCentric.Publishing;
using EventCentric.Tests.Publishing.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EventCentric.Tests.Publishing
{
    [TestClass]
    public class GIVEN_publisher_with_3_events_in_store_that_did_not_start_yet
    {
        protected BusStub bus = new BusStub();
        protected Publisher sut;
        protected EventDaoStub dao = new EventDaoStub();

        public GIVEN_publisher_with_3_events_in_store_that_did_not_start_yet()
        {
            this.sut = new Publisher("Test.FakeStreamType_long-guid-here", this.bus, new ConsoleLogger(), dao, 10, TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public void WHEN_old_subscriber_polls_THEN_no_events_are_delivered()
        {
            var response = this.sut.PollEvents(2, "newSubscriber");

            Assert.IsTrue(response.ErrorDetected);
            Assert.IsFalse(response.NewEventsWereFound);
        }
    }
}
