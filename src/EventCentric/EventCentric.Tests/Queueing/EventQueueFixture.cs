using EventCentric.Queueing;
using EventCentric.Repository;
using EventCentric.Serialization;
using EventCentric.Tests.Helpers;
using EventCentric.Tests.Queueing.Helpers;
using EventCentric.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace EventCentric.Tests.Queueing
{
    [TestClass]
    public class EventQueueFixture : GIVEN_db, IDisposable
    {
        protected EventQueue sut;
        protected GenericTestBus bus;
        protected QueueWriter<EventQueueFixture> writer;

        public EventQueueFixture()
        {
            this.writer = new QueueWriter<EventQueueFixture>(() => new EventQueueDbContext(base.connectionString), new JsonTextSerializer(), new LocalTimeProvider(), new SequentialGuid());
            this.bus = new GenericTestBus();
            this.sut = new EventQueue(this.bus, this.writer);
        }

        [TestMethod]
        public void WHEN_queue_is_empty_THEN_can_enqueue()
        {
            Assert.AreEqual(0, this.bus.Messages.Count);

            this.sut.Send(new TestEvent
            {
                Fact = "Test succeded!",
                StreamId = Guid.Empty
            });

            Assert.AreEqual(1, this.bus.Messages.Count);
        }

        [TestMethod]
        public void WHEN_queue_has_a_stream_THEN_can_append_to_stream()
        {
            this.WHEN_queue_is_empty_THEN_can_enqueue();

            this.sut.Send(new TestEvent
            {
                Fact = "Test succeded! 2",
                StreamId = Guid.Empty,
            });

            Assert.AreEqual(2, this.bus.Messages.Count);
        }

        [TestMethod]
        public void THEN_there_can_be_multiple_streams_in_the_queue()
        {
            this.WHEN_queue_has_a_stream_THEN_can_append_to_stream();

            var newStreamId = Guid.NewGuid();

            this.sut.Send(new TestEvent
            {
                Fact = "Test succeded! 2",
                StreamId = newStreamId,
            });

            this.sut.Send(new TestEvent
            {
                Fact = "Test succeded! 2",
                StreamId = newStreamId,
            });

            Assert.AreEqual(4, this.bus.Messages.Count);
        }

        [TestMethod]
        public void WHEN_multiple_concurrent_producers_wants_to_enqueue_an_event_in_the_same_stream_THEN_they_are_also_enqueued_in_memory_with_a_lock()
        {
            for (int i = 0; i < 3; i++)
            {
                Task.Factory.StartNewLongRunning(() =>
                {
                    this.sut.Send(new TestEvent
                    {
                        Fact = $"Test succeded for {i}th time",
                        StreamId = Guid.Empty
                    });
                });
            }

            //Thread.Sleep(30000);
            // Check debugger
        }

    }
}
