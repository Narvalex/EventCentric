using System.Threading;

namespace EventCentric.Messaging
{
    /// <summary>
    /// In memory versioning, based on: https://msdn.microsoft.com/en-us/library/zs86dyzy(v=vs.110).aspx
    /// </summary>
    public static class InMemoryVersioning
    {
        static InMemoryVersioning()
        {
            version = 0;
        }

        private static long version;

        public static long GetNextVersion() => Interlocked.Increment(ref version);
    }
}
