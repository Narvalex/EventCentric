using System.IO;

namespace EventCentric.Serialization
{
    /// <summary>
    /// Usability overloads for <see cref="ITextSerializer"/>.
    /// </summary>
    public static class TextSerializerExtensions
    {
        /// <summary>
        /// Serializes the given data object as a string.
        /// </summary>
        public static string Serialize<T>(this ITextSerializer serializer, T data)
        {
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Deserializes the specified string into an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="System.InvalidCastException">The deserialized object is not of type <typeparamref name="T"/>.</exception>
        public static T Deserialize<T>(this ITextSerializer serializer, string serialized)
        {
            using (var reader = new StringReader(serialized))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
