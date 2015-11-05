using System;
using System.Text;

namespace EventCentric.Log
{
    public class ConsoleLogger : ILogger
    {
        private readonly LogMessageBuilder messageBuilder = new LogMessageBuilder();

        public void Error(string format, params object[] args)
        {
            Console.WriteLine(this.messageBuilder.BuildMessage("ERROR", format, args));
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            Console.WriteLine(this.messageBuilder.BuildMessage(ex, "ERROR", format, args));
        }

        public void Trace(string[] lines)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine();

            foreach (var line in lines)
                stringBuilder.AppendLine(line);

            stringBuilder.AppendLine();

            Console.WriteLine(stringBuilder.ToString());
        }

        public void Trace(string format, params object[] args)
        {
            Console.WriteLine(this.messageBuilder.BuildMessage("TRACE", format, args));
        }
    }
}
