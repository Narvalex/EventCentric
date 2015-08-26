using System;
using System.Collections.Concurrent;

namespace EventCentric.Publishing
{
    public interface IOldStreamDao
    {
        ConcurrentDictionary<Guid, int> GetStreamsVersionsById();

        int GetStreamCollectionVersion();

        string GetNextEventPayload(Guid streamId, int previousVersion);

        Tuple<Guid, int> GetNextStreamIdAndStreamCollectionVersion(int previousStreamCollectionVersion);
    }
}
