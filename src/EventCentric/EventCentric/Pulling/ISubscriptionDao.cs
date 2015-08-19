using System.Collections.Concurrent;

namespace EventCentric.Pulling
{
    public interface ISubscriptionDao
    {
        /// <summary>
        /// Returns an enumeration of <see cref="Subscription"/> ordered by StreamName.
        /// </summary>
        /// <returns>The enumeration of <see cref="Subscription"/></returns>
        ConcurrentBag<Subscription> GetSubscriptionsOrderedByStreamName();
    }
}
