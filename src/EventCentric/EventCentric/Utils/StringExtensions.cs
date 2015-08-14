namespace EventCentric.Utils
{
    /// <summary>
    /// Provides usability overloads for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        public static string EncodedEmptyString(this string stringObject)
        {
            return "empty";
        }

        public static bool IsEncodedEmptyString(this string stringObject)
        {
            return stringObject == "empty" ? true : false;
        }
    }
}
