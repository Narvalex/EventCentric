using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace EventCentric.Serialization
{
    /// <summary>
    /// Usability overloads for <see cref="ITextSerializer"/>.
    /// </summary>
    public static class TextSerializerExtensionsForDataContract
    {

        public static string SerializeDataContract<T>(this ITextSerializer s, T data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, data);
                return Encoding.UTF8.GetString(memoryStream.ToArray());
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
