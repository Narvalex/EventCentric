using EventCentric.Messaging;
using EventCentric.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace EventCentric.Tests.FiniteStateMachine
{
    [TestClass]
    public class GIVEN_FSM_and_lazy_containers
    {
        protected Bus bus;
        protected TestLazyWorkersContainer workers;
        protected ProcessorNode sut;

        public GIVEN_FSM_and_lazy_containers()
        {
            this.bus = new Bus();
            this.workers = new TestLazyWorkersContainer(this.bus);
            this.sut = new ProcessorNode(this.bus);

            this.bus.Register(this.workers);
            this.bus.Register(this.sut);
        }

        [TestMethod]
        public void WHEN_node_is_created_THEN_can_start()
        {
            ThreadPool.QueueUserWorkItem(_ => this.sut.Start());

            Thread.Sleep(1000);

            Assert.AreEqual(NodeState.Starting, this.sut.State);

            Thread.Sleep(1000);

            Assert.IsTrue(workers.PublisherIsRunning);
            this.workers.Continue();

            Thread.Sleep(1000);

            Assert.IsTrue(workers.ProcessorIsRunning);
            this.workers.Continue();

            Thread.Sleep(1000);
            Assert.IsTrue(workers.PullerIsRunning);
            this.workers.Continue();


            Thread.Sleep(1000);
            Assert.AreEqual(NodeState.UpAndRunning, this.sut.State);
        }

        [TestMethod]
        public void WHEN_starting_and_fatal_error_occurs_THEN_stops_and_throws()
        {
            try
            {
                this.workers.ThrowErrorOnStartup = true;
                this.sut.Start();
            }
            catch (FatalErrorException)
            {
                Console.WriteLine("Expected error throwed");
                return;
            }

            Assert.IsFalse(false, "Should throw error");
        }

        [TestMethod]
        public void WHEN_started_THEN_can_stop()
        {
            this.WHEN_node_is_created_THEN_can_start();

            ThreadPool.QueueUserWorkItem(_ => this.sut.Stop());

            Thread.Sleep(1000);
            Assert.AreEqual(NodeState.Down, this.sut.State);
        }
    }
}

