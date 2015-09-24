﻿using System;

namespace EventCentric.Log
{
    public interface ILogger
    {
        void Trace(string format, params object[] args);

        void Trace(string[] texts);

        void Error(string format, params object[] args);

        void Error(Exception ex, string format, params object[] args);
    }
}
