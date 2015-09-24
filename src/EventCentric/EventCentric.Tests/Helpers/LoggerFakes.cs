using EventCentric.Log;
using System;

namespace EventCentric.Tests.Helpers
{
    public class ConsoleLogger : ILogger
    {
        public void Error(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(format, args);
        }

        public void Trace(string[] texts)
        {
            foreach (var text in texts)
                Console.WriteLine(text);
        }

        public void Trace(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
