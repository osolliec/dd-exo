using System;

namespace DatadogTakeHome.Core.Logger
{
    public interface ILogger
    {
        public void Log(LogLevel level, Exception ex = null, string message = "");
    }

    public enum LogLevel
    {
        Information,
        Error
    }
}
