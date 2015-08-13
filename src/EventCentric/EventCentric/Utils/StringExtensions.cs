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

        public static bool IsAnEncodedEmptyString(this string stringObject, string textToCompare)
        {
            return textToCompare == "empty" ? true : false;
        }
    }
}
