using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventCentric.Tests.Playground
{
    [TestClass]
    public class GroupedEventStreams
    {
        // More info: http://stackoverflow.com/questions/7325278/group-by-in-linq
        [TestMethod]
        public void GIVEN_unordered_list_WHEN_using_linq_THEN_can_group_by_AND_perform_operations_by_groups()
        {
            var list = this.GetEvents();

            var results = list.GroupBy(x => x.StreamId,
                                        x => x,
                                        (key, g) => new
                                        {
                                            StreamId = key,
                                            Events = g.ToList()
                                        });

            foreach (var result in results)
            {
                var arrayOfResults = result.Events.OrderBy(e => e.Version).ToArray();
                // perform async operation on results.
            }

            Assert.AreEqual(3, results.Count());
        }

        private IEnumerable<FakeEvent> GetEvents()
        {
            var id1 = Guid.Empty;
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();

            var list = new List<FakeEvent>();
            for (int i = 0; i < 3; i++)
            {
                list.Add(new FakeEvent { StreamId = id1, Version = i, Payload = $"Id: {id1.ToString()}, Payload: Message {i}" });
                list.Add(new FakeEvent { StreamId = id2, Version = i, Payload = $"Id: {id2.ToString()}, Payload: Message {i}" });
                list.Add(new FakeEvent { StreamId = id3, Version = i, Payload = $"Id: {id3.ToString()}, Payload: Message {i}" });
            }

            return list;
        }
    }



    public class FakeEvent
    {
        public Guid StreamId { get; set; }
        public string Payload { get; set; }
        public int Version { get; set; }
    }
}
