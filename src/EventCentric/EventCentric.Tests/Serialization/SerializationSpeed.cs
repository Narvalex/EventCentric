using EventCentric.Log;
using EventCentric.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EventCentric.Tests.Serialization
{
    [TestClass]
    public class SerializationSpeed
    {
        protected ConsoleLogger log = new ConsoleLogger(true);

        protected JsonTextSerializer textWriterSerializer = new JsonTextSerializer();
        protected JsonTextSerializerWithIdentedFormatting textWriterSerializerWithIndentedFormating = new JsonTextSerializerWithIdentedFormatting();


        [TestMethod]
        public void Should_serialize_a_hundred_thousand_objects()
        {
            var payloadQuantity = 100000;
            Action<long, string> trace = (x, s) => this.log.Trace($"Serialize with {s} {payloadQuantity} objects took: {x} ms");

            trace(this.SerializeWithReflectionAndIndentation(payloadQuantity), "Reflection and indentation");
            trace(this.SerializeWithReflection(payloadQuantity), "Reflection only");
            trace(this.SerializeWithTypeMetadataAndIndentation(payloadQuantity), "TextWriter and indentation");
            trace(this.SerializeWithTypeMetadata(payloadQuantity), "TextWriter only");
        }

        private long SerializeWithReflection(int payloadQuantity)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var consolidatedPayload = CreateConsolidatedPayload(payloadQuantity);

            var serializedConsolidatedPayload = JsonConvert.SerializeObject(consolidatedPayload);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private long SerializeWithReflectionAndIndentation(int payloadQuantity)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var consolidatedPayload = CreateConsolidatedPayload(payloadQuantity);

            var serializedConsolidatedPayload = JsonConvert.SerializeObject(consolidatedPayload, Formatting.Indented);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private long SerializeWithTypeMetadataAndIndentation(int payloadQuantity)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var consolidatedPayload = CreateConsolidatedPayload(payloadQuantity);

            var serializedConsolidatedPayload = this.textWriterSerializerWithIndentedFormating.Serialize(consolidatedPayload);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private long SerializeWithTypeMetadata(int payloadQuantity)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var consolidatedPayload = CreateConsolidatedPayload(payloadQuantity);

            var serializedConsolidatedPayload = this.textWriterSerializer.Serialize(consolidatedPayload);
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }

        private List<TestPayload> CreateConsolidatedPayload(int payloadQuantity)
        {
            var random = new Random();
            var consolidatedPayload = new List<TestPayload>();
            for (int i = 0; i < payloadQuantity; i++)
            {
                var payload = new TestPayload
                {
                    UserId = $"User_{i}",
                    Birthdate = new DateTime(random.Next(1900, 2015), random.Next(1, 12), random.Next(1, 20)),
                    Age = random.Next(1, 70)
                };

                consolidatedPayload.Add(payload);
            }

            return consolidatedPayload;
        }
    }
}

public class TestPayload
{
    public string UserId { get; set; }
    public DateTime Birthdate { get; set; }
    public int Age { get; set; }
}