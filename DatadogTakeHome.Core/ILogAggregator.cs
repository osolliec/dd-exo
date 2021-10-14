using DatadogTakeHome.Core.Model;

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
        /// Get the aggregator's message.
        /// Once called, it should also clear the message from the aggregator.
        /// </summary>
        /// <returns></returns>
        public string GetMessage();
        /// <summary>
        /// Indicates that the current report has a message ready for display.
        /// </summary>
        /// <returns></returns>
        public bool HasMessage();
        /// <summary>
        /// Signals that we've received a bigger timestamp than previously.
        /// </summary>
        /// <param name="maxTimestamp">The max timestamp seen so far by the system.</param>
        public void AdvanceTime(long maxTimestamp);
    }
}
