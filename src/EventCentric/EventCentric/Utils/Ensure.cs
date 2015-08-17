using System;

namespace EventCentric.Utils
{
    public class Ensure
    {
        public static void NotNull<T>(T argument, string argumentName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);
        }

        public static void CastIsValid<T>(T argument, string messageWhenCastIsInvalid) where T : class
        {
            if (argument == null)
                throw new InvalidCastException(messageWhenCastIsInvalid);
        }
    }
}
