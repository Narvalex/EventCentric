using System.Collections.Concurrent;

namespace EventCentric.Pulling
{
    public interface ISubscriptionDao
    {
        /// <summary>
        /// Returns an enumeration of <see cref="SubscribedStream"/> ordered by StreamName.
        /// </summary>
        /// <returns>The enumeration of <see cref="SubscribedStream"/></returns>
        ConcurrentBag<SubscribedStream> GetSubscribedStreamsOrderedByStreamName();

        ConcurrentBag<SubscribedSource> GetSubscribedSources();
    }
}
