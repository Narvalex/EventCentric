using System;

namespace EventCentric.Utils
{
    public interface IUtcTimeProvider
    {
        DateTime Now { get; }

        DateTimeOffset OffSetNow { get; }
    }

    public class UtcTimeProvider : IUtcTimeProvider
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
