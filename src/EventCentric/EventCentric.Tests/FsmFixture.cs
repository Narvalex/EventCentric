using EventCentric.Messaging;
using EventCentric.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace EventCentric.Tests.FsmFixture
{
    [TestClass]
    public class GIVEN_FSM
    {
        protected Bus bus;
        protected TestWorkerContainer workers;
        protected FSM sut;

        public GIVEN_FSM()
        {
            this.bus = new Bus();
            this.workers = new TestWorkerContainer(this.bus);
            this.sut = new FSM(this.bus);

            this.bus.Register(this.workers);
            this.bus.Register(this.sut);
        }

        [TestMethod]
        public void WHEN_node_is_created_THEN_can_start()
        {
            ThreadPool.QueueUserWorkItem(_ => this.sut.Start());

            Thread.Sleep(1000);

            Assert.AreEqual(FSM.NodeState.Starting, this.sut.State);

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
            Assert.AreEqual(FSM.NodeState.UpAndRunning, this.sut.State);
        }

        [TestMethod]
        public void WHEN_started_THEN_can_stop()
        {
            this.WHEN_node_is_created_THEN_can_start();

            ThreadPool.QueueUserWorkItem(_ => this.sut.Stop());

            Thread.Sleep(1000);

            Assert.AreEqual(FSM.NodeState.ShuttingDown, this.sut.State);

            Thread.Sleep(1000);

            Assert.IsFalse(workers.PullerIsRunning);
            this.workers.Continue();

            Thread.Sleep(1000);

            Assert.IsFalse(workers.ProcessorIsRunning);
            this.workers.Continue();

            Thread.Sleep(1000);
            Assert.IsFalse(workers.PublisherIsRunning);
            this.workers.Continue();


            Thread.Sleep(1000);
            Assert.AreEqual(FSM.NodeState.Down, this.sut.State);
        }
    }
}

