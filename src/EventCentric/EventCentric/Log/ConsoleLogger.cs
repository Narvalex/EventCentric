using System;
using System.Text;

namespace EventCentric.Log
{
    public class ConsoleLogger : ILogger
    {
        private readonly bool verbose;
        private readonly LogMessageBuilder messageBuilder = new LogMessageBuilder();

        public bool Verbose => verbose;

        public ConsoleLogger(bool verbose)
        {
            this.verbose = verbose;
            Console.WriteLine(verbose ? "Verbose logging is ENABLED" : "Verbose logging is disabled");
        }

        public void Error(string format)
        {
            Console.WriteLine(this.messageBuilder.BuildMessage("ERROR", format));
        }

        public void Error(string format, string[] lines)
        {
            Console.WriteLine(this.FormatMultipleLines(this.messageBuilder.BuildMessage("ERROR", format), lines));
        }

        public void Error(Exception ex, string format)
        {
            Console.WriteLine(this.messageBuilder.BuildMessage(ex, "ERROR", format));
        }

        public void Error(Exception ex, string format, string[] lines)
        {
            Console.WriteLine(this.FormatMultipleLines(this.messageBuilder.BuildMessage(ex, "ERROR", format), lines));
        }

        public void Log(string format)
        {
            Console.WriteLine(this.messageBuilder.BuildMessage("LOG", format));
        }

        public void Log(string format, string[] lines)
        {
            Console.WriteLine(this.FormatMultipleLines(this.messageBuilder.BuildMessage("LOG", format), lines));
        }

        public void Trace(string format)
        {
            if (this.verbose)
                Console.WriteLine(this.messageBuilder.BuildMessage("TRACE", format));
        }

        public void Trace(string format, string[] lines)
        {
            if (this.verbose)
                Console.WriteLine(this.FormatMultipleLines(this.messageBuilder.BuildMessage("TRACE", format), lines));
        }

        private string FormatMultipleLines(string mainText, string[] lines)
        {
            var stringBuilder = new StringBuilder();

            //stringBuilder.AppendLine();
            stringBuilder.AppendLine(mainText);

            foreach (var line in lines)
                stringBuilder.AppendLine(line);

            //stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }
    }
}
