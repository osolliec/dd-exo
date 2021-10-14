using DatadogTakeHome.Core.Model;
using System.Collections.Generic;
using System.IO;

namespace DatadogTakeHome.Core.Csv
{
    public interface ICsvParser
    {
        /// <summary>
        /// Reads a csv file and outputs its content.
        /// </summary>
        /// <param name="path">The path of the CSV file to read.</param>
        /// <returns></returns>
        IEnumerable<LogLine> Parse(string path);
        /// <summary>
        /// Reads a csv stream and outputs its content.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IEnumerable<LogLine> Parse(Stream stream);
    }
}