using EventCentric.Log;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventCentric.NodeFactory.Log
{
    public class Logger : SignalRBase<LogHub>, ILogger
    {
        private static int _nextMessageId = 0;

        private readonly ConcurrentQueue<Message> messageQueue = new ConcurrentQueue<Message>();
        private static readonly int _processId = Process.GetCurrentProcess().Id;

        private readonly int messageMaxCount = 300;

        private static readonly Logger _logger = new Logger();

        private Logger()
        { }

        public static Logger ResolvedLogger { get { return _logger; } }

        public void Error(string format, params object[] args)
        {
            this.messageQueue.Enqueue(new Message
            {
                id = this.GetMessageId(),
                message = this.BuildMessage("ERROR", format, args)
            });

            this.Flush();
        }

        public void Error(Exception ex, string format, params object[] args)
        {
            this.messageQueue.Enqueue(new Message
            {
                id = this.GetMessageId(),
                message = this.BuildMessage(ex, "ERROR", format, args)
            });

            this.Flush();
        }

        public void Trace(string format, params object[] args)
        {
            this.messageQueue.Enqueue(new Message
            {
                id = this.GetMessageId(),
                message = this.BuildMessage("TRACE", format, args)
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

        private string BuildMessage(string level, string format, params object[] args)
        {
            return string.Format("[{0:00000},{1:00},{2:HH:mm:ss.fff},{3}] {4}",
                                    _processId,
                                    Thread.CurrentThread.ManagedThreadId,
                                    DateTime.Now,
                                    level,
                                    args.Length == 0 ? format : string.Format(format, args));
        }

        private string BuildMessage(Exception ex, string level, string format, params object[] args)
        {
            var stringBuilder = new StringBuilder();
            while (ex != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(ex.ToString());
                ex = ex.InnerException;
            }

            return string.Format("[{0:00000},{1:00},{2:HH:mm:ss.fff},{3}] {4}\nEXCEPTION(S) OCCURRED:{5}",
                                 _processId,
                                 Thread.CurrentThread.ManagedThreadId,
                                 DateTime.Now,
                                 level,
                                 args.Length == 0 ? format : string.Format(format, args),
                                 stringBuilder);
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
