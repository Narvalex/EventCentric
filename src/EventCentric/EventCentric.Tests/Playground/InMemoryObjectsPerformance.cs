using EventCentric.Tests.Playground.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace EventCentric.Tests.Playground.InMemoryObjectsPerformance
{
    [TestClass]
    public class InMemoryObjectsPerformance
    {
        [TestMethod]
        public void WHEN_creating_1million_of_objects_THEN_system_is_responsive()
        {
            var actorsById = new ConcurrentDictionary<string, TestActor>();

            for (int i = 0; i < 1000000; i++)
            {
                var id = Guid.NewGuid();
                actorsById.TryAdd(id.ToString(), new TestActor { Id = id, Payload = string.Format("Actor Id: {0}", id.ToString()) });
            }

            Assert.AreEqual(1000000, actorsById.Count);
        }

        [TestMethod]
        public void WHEN_creating_3_collections_of_100K_of_objects_THEN_system_is_responsive()
        {
            var actorsById1InABag = new ConcurrentBag<TestActor>();
            var actorsById2 = new ConcurrentDictionary<string, TestActor>();
            var actorsById3 = new ConcurrentDictionary<string, TestActor>();

            for (int i = 0; i < 100000; i++)
            {
                var id = Guid.NewGuid();
                actorsById1InABag.Add(new TestActor { Id = id, Payload = string.Format("Actor Id: {0}", id.ToString()) });
            }

            for (int i = 0; i < 100000; i++)
            {
                var id = Guid.NewGuid();
                actorsById2.TryAdd(id.ToString(), new TestActor { Id = id, Payload = string.Format("Actor Id: {0}", id.ToString()) });
            }

            for (int i = 0; i < 100000; i++)
            {
                var id = Guid.NewGuid();
                actorsById3.TryAdd(id.ToString(), new TestActor { Id = id, Payload = string.Format("Actor Id: {0}", id.ToString()) });
            }

            Assert.AreEqual(100000, actorsById1InABag.Count);
            Assert.AreEqual(100000, actorsById2.Count);
            Assert.AreEqual(100000, actorsById3.Count);
        }

        [TestMethod]
        public void WHEN_creating_1K_of_objects_THEN_can_change_property_to_an_object()
        {
            var actors = new ConcurrentBag<TestActor>();

            for (int i = 0; i < 1000; i++)
            {
                var id = Guid.NewGuid();
                actors.Add(new TestActor { Id = id, Payload = string.Format("Actor Id: {0}", id.ToString()) });
            }

            Assert.AreEqual(1000, actors.Count);

            // Change property by a variable that reference the element in the collection
            var actor = actors.First();
            Assert.AreNotEqual(Guid.Empty, actor.Id);

            actor.Id = Guid.Empty;
            Assert.AreEqual(Guid.Empty, actors.First().Id);

            // Change property directly to the element in the collection
            Assert.AreNotEqual(Guid.Empty, actors.Last().Id);
            actors.Last().Id = Guid.Empty;
            Assert.AreEqual(Guid.Empty, actors.Last().Id);

            // Changing property in a loop
            Assert.IsTrue(actors.Where(a => a.Id != Guid.Empty).Any());
            foreach (var a in actors)
            {
                a.Id = Guid.Empty;
            }
            Assert.IsFalse(actors.Where(a => a.Id != Guid.Empty).Any());
        }

        [TestMethod]
        public void WHEN_interating_throuhg_concurrent_bag_THEN_can_add_and_remove_objects_concurrently()
        {
            var actors = new ConcurrentBag<TestActor>();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    var id = Guid.NewGuid();
                    actors.Add(new TestActor { Id = id, Payload = string.Format("Actor Id: {0}", id.ToString()) });
                    Thread.Sleep(10);
                }
            });

            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(100);
                var actorsCount = 0;
                foreach (var actor in actors)
                {
                    actorsCount += 1;
                }
                Console.WriteLine("Count: {0}", actorsCount);
            }
        }
    }
}