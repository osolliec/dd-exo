using DatadogTakeHome.Core.Logger;
using DatadogTakeHome.Core.Model;
using DatadogTakeHome.Core.RequestParser;
using System;
using System.Collections.Generic;

namespace DatadogTakeHome.Core
{
    /// <summary>
    /// The orchestrator ingests a log line and dispatches it to the list of alerts & reports it holds.
    /// It's responsible for making the time advance, it does so by keeping the max timestamp seen so far.
    /// It will also print messages when there are any available.
    /// </summary>
    public class Orchestrator
    {
        private readonly IHttpRequestParser _httpRequestParser;
        private readonly ILogger _logger;

        private readonly IList<ILogAggregator> _logAggregators;

        /// <summary>
        /// Holds messages from alerts & reports to be displayed in a FIFO manner.
        /// </summary>
        private readonly Queue<string> _messages;

        /// <summary>
        /// The maxium timestamp seen so far. It makes the time go forward in our program, and helps close "old" windows of time.
        /// </summary>
        private long _maxTimestampSeenSoFar;

        public Orchestrator(IHttpRequestParser requestParser, ILogger logger, IList<ILogAggregator> logAggregators)
        {
            _httpRequestParser = requestParser;
            _logger = logger;
            _logAggregators = logAggregators;

            _maxTimestampSeenSoFar = -1;

            _messages = new Queue<string>();

            foreach (var aggregator in logAggregators)
            {
                aggregator.RegisterMessageQueue(_messages);
            }
        }

        /// <summary>
        /// Ingest the log line, and pass it to all alerts & reports.
        /// Will ignore bad log lines, if we can't parse their sections.
        /// </summary>
        /// <param name="logLine"></param>
        public void Collect(LogLine logLine)
        {
            if (!_httpRequestParser.TryParse(logLine.Request, out var parsedRequest))
            {
                _logger.Log(LogLevel.Error, null, $"Could not parse request {logLine.Request}. Ignoring.");
                return;
            }

            long previousTimestamp = _maxTimestampSeenSoFar;
            _maxTimestampSeenSoFar = Math.Max(_maxTimestampSeenSoFar, logLine.TimestampSeconds);

            foreach (var _logAggregator in _logAggregators)
            {
                if (_maxTimestampSeenSoFar > previousTimestamp)
                {
                    // Advance the time so the downstream operators can close their window.
                    _logAggregator.AdvanceTime(_maxTimestampSeenSoFar);
                }

                _logAggregator.Collect(logLine, parsedRequest);
            }
        }

        /// <summary>
        /// If any message is present in the orchestrator, display them and remove them from its message queue.
        /// </summary>
        public void DisplayMessages()
        {
            while (_messages.Count > 0)
            {
                string message = _messages.Dequeue();
                _logger.Log(LogLevel.Information, null, message);
            }
        }
    }
}
