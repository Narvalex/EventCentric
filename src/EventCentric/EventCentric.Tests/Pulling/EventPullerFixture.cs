using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Pulling;
using EventCentric.Serialization;
using EventCentric.Tests.Pulling.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;

namespace EventCentric.Tests.Pulling.EventPullerFixture
{
    [TestClass]
    public class GIVEN_event_puller
    {
        protected Guid streamId1;
        protected TestBus bus;
        protected EventPuller sut;
        protected TestSubscriptionDaoWithSingleResult dao;
        protected Func<TestHttpClientWithSingleResult> httpFactory;
        protected JsonTextSerializer serializer;

        public GIVEN_event_puller()
        {
            this.streamId1 = Guid.NewGuid();
            this.serializer = new JsonTextSerializer();
            this.bus = new TestBus();
            this.dao = new TestSubscriptionDaoWithSingleResult(this.streamId1);
            this.httpFactory = () => new TestHttpClientWithSingleResult(this.streamId1);
            this.sut = new EventPuller(this.bus, this.dao, this.httpFactory, this.serializer);
        }

        [TestMethod]
        public void WHEN_there_is_a_single_subscription_with_pending_event_THEN_enters_busy_state_and_pulls()
        {
            this.sut.Handle(new StartEventPuller());

            for (int i = 0; i < 100; i++)
            {
                if (this.bus.Messages.Count == 2)
                {
                    Assert.IsTrue(this.bus.Messages.Where(m =>
                                {
                                    var @event = m as NewIncomingEvent;
                                    return @event != null;
                                })
                                .Any());
                    return;
                }

                Thread.Sleep(100);
            }

            throw new TimeoutException();
        }

        [TestMethod]
        public void WHEN_response_has_no_new_events_THEN_sets_subscriptions_to_not_busy()
        {
            this.httpFactory = () => new TestHttpClientWithSingleResult(this.streamId1, false);
            this.sut = new EventPuller(this.bus, this.dao, this.httpFactory, this.serializer);

            Assert.IsFalse(TestSubscriptionDaoWithSingleResult.Subscriptions.Single().IsBusy);

            this.sut.Handle(new StartEventPuller());

            for (int i = 0; i < 100; i++)
            {
                if (this.bus.Messages.Count == 1
                    && TestSubscriptionDaoWithSingleResult.Subscriptions.Single().IsBusy)
                    break;
                else
                    Thread.Sleep(100);
            }

            for (int i = 0; i < 100; i++)
            {
                if (!TestSubscriptionDaoWithSingleResult.Subscriptions.Single().IsBusy)
                {
                    Assert.AreEqual(1, this.bus.Messages.Count);
                    return;
                }

                Thread.Sleep(100);
            }

            throw new TimeoutException();
        }

        [TestMethod]
        public void WHEN_incoming_event_has_been_processed_THEN_updates_subscription_status()
        {
            // TODO
        }

        [TestMethod]
        public void WHEN_incoming_event_is_marked_as_poisoned_THEN_updates_subscription_status()
        {
            // TODO
        }

        [TestMethod]
        public void WHEN_new_subscription_was_performed_THEN_adds_new_subscription_for_polling()
        {
            // TODO
        }
    }
}
