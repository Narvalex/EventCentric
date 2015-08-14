using System;
using System.Collections.Generic;

namespace EventCentric.Publishing
{
    public interface IStreamDao
    {
        IEnumerable<KeyValuePair<Guid, int>> GetStreamsVersionsById();

        string GetNextEventPayload(Guid streamId, int previousVersion);
    }
}
