using System.Collections.Generic;

namespace DatadogTakeHome.Core.Stats
{
    /// <summary>
    /// Aggregates raw CSV data into useful statistics.
    /// </summary>
    public class LogContainer
    {
        /// <summary>
        /// Stores status code (200, 404, 500, etc) hit counts.
        /// We use a sorted dictionary here, so that when we access the status codes (to print them), they are always
        /// ordered from smallest to biggest. This makes troubleshooting easier.
        /// </summary>
        private SortedDictionary<int, long> _statusCodeHits;
        /// <summary>
        /// Stores section hits, a section is what's before the second slash, eg: "/api"
        /// </summary>
        private Dictionary<string, long> _sectionHits;
        /// <summary>
        /// Stores total hits of the system
        /// </summary>
        private long _totalHits;

        public LogContainer()
        {
            _statusCodeHits = BuildCommonStatusCodes();
            _sectionHits = new Dictionary<string, long>();
            _totalHits = 0;
        }

        /// <summary>
        /// Create and fill a SortedDictionary of common http codes. We always expect to see them, even when they have a count of 0.
        /// </summary>
        /// <returns>SortedDictionary of common codes with 0 count</returns>
        private SortedDictionary<int, long> BuildCommonStatusCodes()
        {
            var result = new SortedDictionary<int, long>();
            for (int i = 2; i <= 5; i++)
            {
                result[i * 100] = 0;
            }
            return result;
        }

        /// <summary>
        /// Resets the memory of this container. It's equivalent to creating a new LogContainer, without creating a new object.
        /// </summary>
        public void Clear()
        {
            _statusCodeHits = BuildCommonStatusCodes();
            _sectionHits = new Dictionary<string, long>();
            _totalHits = 0;
        }

        public void CollectTotalHits()
        {
            _totalHits++;
        }
        public long GetTotalHits()
        {
            return _totalHits;
        }
        public void CollectStatusCode(int statusCode)
        {
            if (!_statusCodeHits.ContainsKey(statusCode))
            {
                _statusCodeHits[statusCode] = 0;
            }
            _statusCodeHits[statusCode]++;
        }

        public SortedDictionary<int, long> GetStatusCodes()
        {
            return _statusCodeHits;
        }

        public void CollectSectionHits(string section)
        {
            if (!_sectionHits.ContainsKey(section))
            {
                _sectionHits[section] = 0;
            }

            _sectionHits[section]++;
        }

        public Dictionary<string, long> GetSectionHits()
        {
            return _sectionHits;
        }
    }
}
