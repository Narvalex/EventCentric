using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

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

        public static string SerializeDataContract<T>(this ITextSerializer s, T data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, data);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
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

        public static T DeserializeDataContract<T>(this ITextSerializer s, string serialized)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serialized)))
            {
                return (T)serializer.ReadObject(memoryStream);
            }
        }
    }
}
