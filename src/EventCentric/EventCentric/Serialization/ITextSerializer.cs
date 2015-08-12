using System.IO;

namespace EventCentric.Serialization
{
    /// <summary>
    /// Interface for serializers that can read/write an object graph to a stream.
    /// </summary>
    public interface ITextSerializer
    {
        /// <summary>
        /// Serializes an object graph to a text reader.
        /// </summary>
        void Serialize(TextWriter writer, object graph);

        /// <summary>
        /// Deserializes an object graph from the scpecified text reader.
        /// </summary>
        object Deserialize(TextReader reader);
    }
}
