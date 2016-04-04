using EventCentric.Log;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace EventCentric.Factory
{
    public class SignalRLogger : SignalRBase<LogHub>, ILogger
    {
        public static bool _Verbose { get; private set; }

        public bool Verbose => _Verbose;

        private static int _nextMessageId = 0;

        private readonly ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();

        private readonly int messageMaxCount = 300;

        private static readonly SignalRLogger _logger = new SignalRLogger();

        private readonly LogMessageBuilder messageBuilder = new LogMessageBuilder();

        private SignalRLogger() { }

        public static SignalRLogger GetResolvedSignalRLogger(bool verbose = true)
        {
            SignalRLogger._Verbose = verbose;
            return _logger;
        }

        public static SignalRLogger GetResolvedSignalRLogger() => _logger;

        public void Error(string format)
        {
            this.FlushMessage(new Message
            {
                id = this.GetMessageId(),
                message = this.messageBuilder.BuildMessage("ERROR", format)
            });
        }

        public void Error(string format, string[] lines)
        {
            this.FlushMessage(new Message
            {
                id = this.GetMessageId(),
                message = this.FormatMultipleLines(this.messageBuilder.BuildMessage("ERROR", format), lines)
            });
        }

        public void Error(Exception ex, string format)
        {
            this.FlushMessage(new Message
            {
                id = this.GetMessageId(),
                message = this.messageBuilder.BuildMessage(ex, "ERROR", format)
            });
        }

        public void Error(Exception ex, string format, string[] lines)
        {
            this.FlushMessage(new Message
            {
                id = this.GetMessageId(),
                message = this.FormatMultipleLines(this.messageBuilder.BuildMessage(ex, "ERROR", format), lines)
            });
        }

        public void Trace(string format)
        {
            if (_Verbose)
                this.FlushMessage(new Message
                {
                    id = this.GetMessageId(),
                    message = this.messageBuilder.BuildMessage("TRACE", format)
                });
        }

        public void Trace(string format, string[] lines)
        {
            if (_Verbose)
                this.FlushMessage(new Message
                {
                    id = this.GetMessageId(),
                    message = this.FormatMultipleLines(this.messageBuilder.BuildMessage("TRACE", format), lines)
                });
        }

        public void Log(string format)
        {
            this.FlushMessage(new Message
            {
                id = this.GetMessageId(),
                message = this.messageBuilder.BuildMessage("LOG", format)
            });
        }

        public void Log(string format, string[] lines)
        {
            this.FlushMessage(new Message
            {
                id = this.GetMessageId(),
                message = this.FormatMultipleLines(this.messageBuilder.BuildMessage("LOG", format), lines)
            });
        }

        private void FlushMessage(Message msg)
        {
            ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(_ =>
            {
                this.messageQueue.Enqueue(msg);
                lock (this)
                {
                    if (messageQueue.Any())
                        foreach (var message in this.messageQueue)
                            this.Hub.Clients.All.notify(message);

                    // Clean a little bit
                    Message nullMessage;
                    while (this.messageQueue.Count >= this.messageMaxCount)
                        this.messageQueue.TryDequeue(out nullMessage);
                }
            }), null);
        }

        private int GetMessageId() => Interlocked.Increment(ref _nextMessageId);

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

    /// <summary>
    /// JSON camel casing format.
    /// </summary>
    public class Message
    {
        public int id { get; set; }
        public string message { get; set; }
    }
}
