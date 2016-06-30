using EventCentric.Utils;
using System;

namespace EventCentric.Persistence
{
    public abstract class EventStoreBase
    {
        protected readonly string streamType;

        protected EventStoreBase(string streamType)
        {
            Ensure.NotNullNeitherEmtpyNorWhiteSpace(streamType, nameof(streamType));
            if (streamType.Length > 40)
                throw new InvalidOperationException($"The stream type '{streamType}' has a length of {streamType.Length}. The maximun lenght of an stream type is 40");

            this.streamType = streamType;
        }

        protected string GetConsistencyPercentage(long consumerVersion, long producerVersion)
        {
            if (producerVersion == 0) return "100%";

            double fullPercentage = (consumerVersion * 100) / producerVersion;
            return $"{Math.Round(fullPercentage, 2)}%";
        }
    }
}
