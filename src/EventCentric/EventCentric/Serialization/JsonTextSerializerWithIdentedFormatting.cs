using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization;

namespace EventCentric.Serialization
{
    public class JsonTextSerializerWithIdentedFormatting : ITextSerializer
    {
        private readonly JsonSerializer serializer;

        public JsonTextSerializerWithIdentedFormatting()
        : this(JsonSerializer.Create(new JsonSerializerSettings
        {
            // Allows deserializing to the actual runtime type
            TypeNameHandling = TypeNameHandling.All,
            // In a version resilient way
            TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
        }))
        { }

        public JsonTextSerializerWithIdentedFormatting(JsonSerializer serializer)
        {
            this.serializer = serializer;
        }

        public void Serialize(TextWriter writer, object graph)
        {
            var jsonWriter = new JsonTextWriter(writer);

            jsonWriter.Formatting = Formatting.Indented;

            this.serializer.Serialize(jsonWriter, graph);

            // We don't close the stream as it's owned by the message.
            writer.Flush();
        }

        public object Deserialize(TextReader reader)
        {
            var jsonReader = new JsonTextReader(reader);

            try
            {
                return this.serializer.Deserialize(jsonReader);
            }
            catch (JsonSerializationException e)
            {
                // Wrap in a standard .NET exception.
                throw new SerializationException(e.Message, e);
            }
        }
    }
}
