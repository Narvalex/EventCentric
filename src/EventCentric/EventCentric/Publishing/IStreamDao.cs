using System;
using System.Collections.Concurrent;

namespace EventCentric.Publishing
{
    public interface IStreamDao
    {
        ConcurrentDictionary<Guid, int> GetStreamsVersionsById();

        string GetNextEventPayload(Guid streamId, int previousVersion);
    }
}
