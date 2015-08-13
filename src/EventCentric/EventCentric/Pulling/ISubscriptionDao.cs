using System.Collections.Generic;

namespace EventCentric.Pulling
{
    public interface ISubscriptionDao
    {
        /// <summary>
        /// Returns an enumeration of <see cref="Subscription"/> ordered by StreamName.
        /// </summary>
        /// <returns>The enumeration of <see cref="Subscription"/></returns>
        IEnumerable<Subscription> GetSubscriptionsOrderedByStreamName();
    }
}
