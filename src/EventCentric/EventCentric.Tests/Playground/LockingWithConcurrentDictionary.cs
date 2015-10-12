using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Tests.Playground
{
    [TestClass]
    public class LockingWithConcurrentDictionary
    {
        private readonly ConcurrentDictionary<string, object> streamLocksById;

        public LockingWithConcurrentDictionary()
        {
            this.streamLocksById = new ConcurrentDictionary<string, object>();
        }


        [TestMethod]
        public void THEN_lock_with_string_key_and_object_lock()
        {

            var t1 = new Task(() =>
            {
                var id = Guid.Empty;
                this.HandleWithLock(id, $"Task 1 with id {id.ToString()}");
            });

            var t2 = new Task(() =>
            {
                var id = Guid.Empty;
                this.HandleWithLock(id, $"Task 2 with id {id.ToString()}");
            });

            var t3 = new Task(() =>
            {
                var id = Guid.NewGuid();
                this.HandleWithLock(id, $"Task 3 with id {id.ToString()}");
            });

            t1.Start();
            t2.Start();
            t3.Start();

            Task.WaitAll(t1, t2, t3);
        }

        private void HandleWithLock(Guid id, string message)
        {
            lock (this.streamLocksById.GetOrAdd(id.ToString(), new object()))
            {
                Thread.Sleep(1000);
                Console.WriteLine(message);
            }
        }
    }
}
