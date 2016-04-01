using System;

namespace EventCentric.Log
{
    public interface ILogger
    {
        bool Verbose { get; }

        void Trace(string text);
        void Trace(string text, string[] lines);

        void Log(string text);
        void Log(string text, string[] lines);

        void Error(string text);
        void Error(string text, string[] lines);

        void Error(Exception ex, string text);
        void Error(Exception ex, string text, string[] lines);
    }
}
