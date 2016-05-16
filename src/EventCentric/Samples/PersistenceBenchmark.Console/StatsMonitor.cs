using EventCentric.EventSourcing;
using EventCentric.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace PersistenceBenchmark.ConsoleHost
{
    public class StatsMonitor
    {
        private readonly List<Tuple<string, Func<EventStoreStats>>> stores = new List<Tuple<string, Func<EventStoreStats>>>();

        public void Add<T>(InMemoryEventStore<T> store) where T : class, IEventSourced
        {
            this.stores.Add(new Tuple<string, Func<EventStoreStats>>(typeof(T).Name, store.GetStats));
        }

        public void PrintStats()
        {
            foreach (var s in stores)
            {
                var storeName = s.Item1;
                var stats = s.Item2.Invoke();
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine($"| Stats of {storeName}");
                sb.AppendLine($"| Events count  {stats.EventsCount}");
                sb.AppendLine($"| Inbox count   {stats.InboxCount}");
                sb.AppendLine();
                Console.WriteLine(sb.ToString());
            }

        }
    }
}
