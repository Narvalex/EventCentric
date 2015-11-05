using System;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace EventCentric.Log
{
    public class LogMessageBuilder
    {
        private static readonly int _processId = Process.GetCurrentProcess().Id;

        public string BuildMessage(string level, string format, params object[] args)
        {
            return string.Format("[{0:00000},{1:00} {2:HH:mm:ss.fff} {3}] {4}",
                                    _processId,
                                    Thread.CurrentThread.ManagedThreadId,
                                    DateTime.Now,
                                    level,
                                    args.Length == 0 ? format : string.Format(format, args));
        }

        public string BuildMessage(Exception ex, string level, string format, params object[] args)
        {
            var stringBuilder = new StringBuilder();
            while (ex != null)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(ex.ToString());
                ex = ex.InnerException;
            }

            return string.Format("[{0:00000},{1:00} {2:HH:mm:ss.fff} {3}] {4}\nEXCEPTION(S) OCCURRED:{5}",
                                 _processId,
                                 Thread.CurrentThread.ManagedThreadId,
                                 DateTime.Now,
                                 level,
                                 args.Length == 0 ? format : string.Format(format, args),
                                 stringBuilder);
        }
    }
}
