using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventCentric.Tests.Publishing.EventPublisherFixture
{
    [TestClass]
    public class GIVEN_event_publisher
    {
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
