using DatadogTakeHome.Core.Datastructures;
using DatadogTakeHome.Core.Model;
using System.Collections.Generic;
using System.Text;

namespace DatadogTakeHome.Core.Stats
{
    /// <summary>
    /// This is the class that handles the guidelines' "For every 10 seconds of logs, display debug information".
    ///
    /// Every periodDurationSeconds seconds, it will:
    ///     1° Display information about the previous period
    ///     2° Clear the previous information
    ///
    /// It accepts a period duration as parameter should we want to change the 10 seconds to something else.
    ///
    /// It will accept late events (events later than the maxTimestamp), but no later than maxTimestamp - periodDurationSeconds.
    /// </summary>
    public class PeriodicSummaryReport : ILogAggregator
    {
        private static int MAX_NUMBER_OF_SECTIONS_TO_DISPLAY = 5;

        /// <summary>
        /// The duration of the period of this PeriodicSummaryReport, in seconds.
        /// </summary>
        private readonly int _windowDurationSeconds;

        /// <summary>
        /// The container to aggregate the raw CSV data.
        /// </summary>
        private LogContainer _logContainer;

        /// <summary>
        /// The time when the previous report was made, -1 if never.
        /// </summary>
        private long _windowStartTime = -1;

        /// <summary>
        /// Holds our report message.
        /// </summary>
        private StringBuilder _message;

        /// <summary>
        /// Build a PeriodicSummaryReport.
        /// </summary>
        /// <param name="windowDurationSeconds">The duration in which the statistics should be gathered.</param>
        public PeriodicSummaryReport(int windowDurationSeconds)
        {
            _windowDurationSeconds = windowDurationSeconds;
            _logContainer = new LogContainer();
            _message = new StringBuilder();
        }

        /// <summary>
        /// Aggregate the raw CSV data into a human friendly display message.
        ///
        /// The way it works is the following:
        /// 1° Checks whether we can publish the message (in other words, if the time for next report is overdue).
        /// 2° Aggregates data if it is not too late (we reject messages that are older than the current window of time)
        ///
        /// </summary>
        /// <param name="logLine"></param>
        /// <param name="parsedRequest"></param>
        /// <param name="maxTimestamp"></param>
        public void Collect(LogLine logLine, ParsedRequest parsedRequest)
        {
            // We ignore messages that are too late (before this window start time). Otherwise it would make our reports inaccurate.
            if (logLine.TimestampSeconds >= _windowStartTime)
            {
                _logContainer.CollectSectionHits(parsedRequest.Section);
                _logContainer.CollectStatusCode(logLine.HttpStatusCode);
                _logContainer.CollectTotalHits();
            }
        }

        public void AdvanceTime(long maxTimestamp)
        {
            if (_windowStartTime == -1)
            // the first time our program runs, we need to initialize the windowStartTime of the report.
            {
                _windowStartTime = maxTimestamp;
            }
            else if (maxTimestamp - _windowStartTime >= _windowDurationSeconds)
            // our report is overdue, we need to publish it
            {
                BuildReport(_windowStartTime, maxTimestamp);
                // Reset the container's contents.
                _logContainer.Clear();

                _windowStartTime = maxTimestamp;
            }
        }

        /// <summary>
        /// Return the current message and clear it from the report's memory.
        /// </summary>
        /// <returns></returns>
        public string GetMessage()
        {
            var message = _message.ToString();
            _message.Clear();
            return message;
        }

        public bool HasMessage()
        {
            return _message.Length > 0;
        }

        /// <summary>
        /// Store into the StringBuilder the information contained in the LogContainer.
        /// </summary>
        /// <param name="windowStartTime"></param>
        /// <param name="windowEndTime"></param>
        private void BuildReport(long windowStartTime, long windowEndTime)
        {
            var previousInclusive = DateFormatter.FormatDate(windowStartTime);
            var currentExclusive = DateFormatter.FormatDate(windowEndTime);

            _message.Append($"REPORT - FROM {previousInclusive} TO {currentExclusive} EXCLUSIVE\n");
            _message.Append($"TOTAL HITS: {_logContainer.GetTotalHits()} \n");
            _message.Append("TOP 5 SECTIONS HITS: \n");

            var top5Sections = GetTopKSectionsWithCount(_logContainer.GetSectionHits(), MAX_NUMBER_OF_SECTIONS_TO_DISPLAY);

            foreach (var section in top5Sections)
            {
                _message.Append($"SECTION: {section.Item2} HITS: {section.Item1} \n");
            }

            foreach (var code in _logContainer.GetStatusCodes())
            {
                _message.Append($"HTTP_CODE: {code.Key} HITS: {code.Value} \n");
            }
        }

        /// <summary>
        /// Returns the top k sections in o(nlogk) instead of o(nlogn) if we sorted the whole list.
        /// </summary>
        /// <param name="sectionHits"></param>
        /// <returns></returns>
        private static List<(long, string)> GetTopKSectionsWithCount(Dictionary<string, long> sectionHits, int k)
        {
            var result = new List<(long, string)>();

            var priorityQueue = new MinPriorityQueue<string>(k);

            foreach (var kvp in sectionHits)
            {
                if (priorityQueue.Size == k)
                {
                    var min = priorityQueue.Peek();
                    if (kvp.Value > min.Item1)
                    {
                        // remove the min
                        priorityQueue.ExtractMin();
                        // add the bigger one
                        priorityQueue.Insert(kvp.Value, kvp.Key);
                    }
                }
                else
                {
                    priorityQueue.Insert(kvp.Value, kvp.Key);
                }
            }

            while (priorityQueue.Size > 0)
            {
                var min = priorityQueue.ExtractMin();
                result.Insert(0, min);
            }

            return result;
        }
    }
}
