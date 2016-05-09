namespace System
{
    public static class MiscExtensions
    {
        /// <summary>
        /// More info: https://en.wikipedia.org/wiki/Tee_(command)
        /// </summary>
        public static T Tee<T>(this T obj, Action<T> action)
        {
            action.Invoke(obj);
            return obj;
        }
    }
}
