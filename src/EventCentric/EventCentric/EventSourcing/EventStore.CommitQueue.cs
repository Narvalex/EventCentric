using EventCentric.Messaging.Events;
using EventCentric.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EventCentric.EventSourcing
{
    public partial class EventStore<T>
    {
        private readonly ConcurrentQueue<Func<IEventStoreDbContext, Tuple<string, long>>> pendingCommits = new ConcurrentQueue<Func<IEventStoreDbContext, Tuple<string, long>>>();

        private void TryCommitEvents()
        {
            Func<IEventStoreDbContext, Tuple<string, long>> commitAction;
            while (!this.stopping)
            {
                if (!this.pendingCommits.IsEmpty)
                {
                    List<Tuple<string, long>> incomingEvents = new List<Tuple<string, long>>();
                    using (var context = this.contextFactory.Invoke(false))
                    {
                        while (this.pendingCommits.TryDequeue(out commitAction)) // needs a limit, like a hundred, or a thousand
                        {
                            incomingEvents.Add(commitAction.Invoke(context));
                        }
                        context.SaveChanges(); // needs to be reliable... catch all errors from Handler needs to be ported here
                    }

                    this.bus.Publish(new EventStoreHasBeenUpdated(this.eventCollectionVersion));
                    this.bus.Publish(new IncomingEventsHasBeenProcessed(incomingEvents));
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }
    }
}
