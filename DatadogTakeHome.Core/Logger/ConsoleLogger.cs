using System;

namespace DatadogTakeHome.Core.Logger
{
    /// <summary>
    /// A helper class to print to the console. In the real world I would use a library, but for this simple example it should be enough.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public void Log(LogLevel level, Exception ex = null, string message = "")
        {
            string formatted = $"{message} {ex?.Message}";
            switch(level)
            {
                case LogLevel.Error:
                    Console.Error.WriteLine(formatted);
                    break;
                case LogLevel.Information:
                    Console.Out.WriteLine(formatted);
                    break;
            }
        }
    }
}
