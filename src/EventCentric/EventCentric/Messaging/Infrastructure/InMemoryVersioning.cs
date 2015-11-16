using System;

namespace EventCentric.Messaging.Infrastructure
{
    /// <summary>
    /// In memory versioning, based on: https://msdn.microsoft.com/en-us/library/system.datetime.ticks.aspx
    /// </summary>
    public class InMemoryVersioning
    {
        public static long GetNextVersion()
        {
            return DateTime.UtcNow.Ticks;
        }
    }
}
