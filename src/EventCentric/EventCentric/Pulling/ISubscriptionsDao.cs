using System.Collections.Generic;

namespace EventCentric.Pulling
{
    public interface ISubscriptionsDao
    {
        /// <summary>
        /// Returns an enumeration of <see cref="Subscription"/> ordered by StreamName.
        /// </summary>
        /// <returns>The enumeration of <see cref="Subscription"/></returns>
        IEnumerable<Subscription> GetSubscriptionsOrderedByStreamName();
    }
}
