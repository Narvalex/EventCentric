using System;

namespace EventCentric.Utils
{
    public interface ITimeProvider
    {
        DateTime Now { get; }

        DateTimeOffset OffSetNow { get; }
    }

    public class LocalTimeProvider : ITimeProvider
    {
        public DateTime Now
        {
            get
            {
                return DateTime.Now;
            }
        }

        public DateTimeOffset OffSetNow
        {
            get
            {
                return DateTimeOffset.Now;
            }
        }
    }

    public class UtcTimeProvider : ITimeProvider
    {
        public DateTime Now
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public DateTimeOffset OffSetNow
        {
            get
            {
                return DateTimeOffset.UtcNow;
            }
        }
    }

}
