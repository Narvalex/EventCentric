using EventCentric.Database;
using EventCentric.Messaging.Commands;
using EventCentric.Messaging.Events;
using EventCentric.Publishing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Tests.Publishing.Helpers;
using EventCentric.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Linq;

namespace EventCentric.Tests.Publishing.EventPublisherFixture
{
    [TestClass]
    public class GIVEN_event_publisher : IDisposable
    {
        protected string connectionString;
        protected TestBus bus;
        protected StreamDao dao;
        protected EventPublisher<TestAggregate> sut;

        public GIVEN_event_publisher()
        {
            this.connectionString = ConfigurationManager.AppSettings["defaultConnection"];
            EventStoreDbInitializer.CreateDatabaseObjects(connectionString, true);
            this.bus = new TestBus();
            this.dao = new StreamDao(() => new ReadOnlyStreamDbContext(this.connectionString));
            this.sut = new EventPublisher<TestAggregate>(this.bus, this.dao, new JsonTextSerializer());
        }

        public void Dispose()
        {
            SqlClientLite.DropDatabase(this.connectionString);
        }

        [TestMethod]
        public void THEN_can_create_db_for_stream_dao()
        {
            using (var context = new ReadOnlyStreamDbContext(this.connectionString))
            {
            }

            // If got this far means that is was a successfull operation.
        }

        [TestMethod]
        public void WHEN_starting_and_no_stream_is_found_THEN_continues()
        {
            Assert.AreEqual(0, this.bus.Messages.Count);
            this.sut.Handle(new StartEventPublisher());
            Assert.AreEqual(1, this.bus.Messages.Count);
            Assert.AreEqual(typeof(EventPublisherStarted), this.bus.Messages.Single().GetType());
        }

        [TestMethod]
        public void WHEN_no_stream_to_publish_and_poll_occurs_THEN_replies_with_no_new_event_found()
        {
            var encondedEmptyGuid = Guid.Empty.ToString();

            this.sut.Handle(new StartEventPublisher());
            var response = this.sut.PollEvents(new PollRequest(encondedEmptyGuid, 1, encondedEmptyGuid, 2, encondedEmptyGuid, 3, encondedEmptyGuid, 4, encondedEmptyGuid, 5));

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Events.Where(e => e.IsNewEvent).Any());
        }

        [TestMethod]
        public void WHEN_starting_THEN_retrieves_all_streams_status_to_notify_that_it_started()
        {
            // TODO:
        }

        [TestMethod]
        public void WHEN_event_store_has_been_updated_THEN_checks_if_the_stream_already_exists_in_in_memory_collection()
        {
            // TODO:
        }

        [TestMethod]
        public void WHEN_an_stream_was_updated_but_notification_arrives_late_before_a_new_one_THEN_no_op()
        {
            // TODO:
        }

        [TestMethod]
        public void WHEN_an_stream_was_updated_but_notification_arrives_on_time_THEN_updates_stream_status()
        {
            // TODO:
        }

        [TestMethod]
        public void WHEN_incoming_http_request_arrives_to_poll_and_new_events_are_found_for_all_streams_THEN_events_are_dispatched()
        {
            // TODO:
        }

        [TestMethod]
        public void WHEN_incoming_http_request_arrives_to_poll_and_no_events_are_found_for_any_stream_THEN_notifies_no_new_event_where_found()
        {
            // TODO:
        }

        [TestMethod]
        public void WHEN_incoming_http_request_arrives_to_poll_and_some_events_are_found_for_given_streams_THEN_notifies_no_new_event_where_found_and_dispath_events_found()
        {
            // TODO:
        }

        ///-------------------------
        /// Test template
        ///-------------------------
        //[TestMethod]
        //public void WHEN_x_THEN_y()
        //{

        //}
    }
}
