using EventCentric.Log;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.Factory
{
    public class Logger : SignalRBase<LogHub>, ILogger
    {
        private static int _nextMessageId = 0;

        private readonly ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();

        private readonly int messageMaxCount = 300;

        private static readonly Logger _logger = new Logger();

        private readonly LogMessageBuilder messageBuilder = new LogMessageBuilder();

        private Logger()
        { }

        public static Logger ResolvedLogger { get { return _logger; } }

        public void Error(string format, params object[] args)
        {
            this.messageQueue.Enqueue(new Message
            {
                id = this.GetMessageId(),
                message = this.messageBuilder.BuildMessage("ERROR", format, args)
            });

            this.Flush();
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            this.messageQueue.Enqueue(new Message
            {
                id = this.GetMessageId(),
                message = this.messageBuilder.BuildMessage(ex, "ERROR", format, args)
            });

            this.Flush();
        }

        public void Trace(string format, params object[] args)
        {
            this.messageQueue.Enqueue(new Message
            {
                id = this.GetMessageId(),
                message = this.messageBuilder.BuildMessage("TRACE", format, args)
            });

            this.Flush();
        }

        private void Flush()
        {
            Task.Factory.StartNew(() =>
            {
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
            }, TaskCreationOptions.PreferFairness);
        }

        private int GetMessageId()
        {
            return Interlocked.Increment(ref _nextMessageId);
        }



        public void Trace(params string[] lines)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine();

            foreach (var line in lines)
                stringBuilder.AppendLine(line);

            stringBuilder.AppendLine();

            this.messageQueue.Enqueue(new Message
            {
                id = this.GetMessageId(),
                message = stringBuilder.ToString()
            });
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
