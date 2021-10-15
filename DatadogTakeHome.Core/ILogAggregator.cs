using DatadogTakeHome.Core.Model;
using System.Collections.Generic;

namespace DatadogTakeHome.Core
{
    /// <summary>
    /// The interface that alerts or reports have to extend from.
    /// </summary>
    public interface ILogAggregator
    {
        /// <summary>
        /// Collecting a log line means aggregating the raw CSV data into any relevant structure to build the message.
        /// </summary>
        /// <param name="logLine">The raw log line read from CSV input.</param>
        /// <param name="parsedRequest">The parsed request which contains Section information</param>
        public void Collect(LogLine logLine, ParsedRequest parsedRequest);
        /// <summary>
        /// Register a message queue to publish messages when the ILogAggregator needs to publish messages.
        /// </summary>
        /// <param name="messageQueue"></param>
        public void RegisterMessageQueue(Queue<string> messageQueue);
        /// <summary>
        /// Signals that we've received a bigger timestamp than previously.
        /// </summary>
        /// <param name="maxTimestamp">The max timestamp seen so far by the system.</param>
        public void AdvanceTime(long maxTimestamp);
    }
}
