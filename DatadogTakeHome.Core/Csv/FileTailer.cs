using CsvHelper;
using CsvHelper.Configuration;
using DatadogTakeHome.Core.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace DatadogTakeHome.Core.Csv
{
    public class FileTailer
    {
        /// <summary>
        /// Continuously read the file, eg like tail -f.
        /// This action is blocking.
        ///
        /// It won't detect a complete file change (like a log rotate), and should be improved for this use case.
        ///
        /// See https://stackoverflow.com/a/24993767
        /// </summary>
        /// <param name="path"></param>
        public IEnumerable<LogLine> ContinuouslyReadFile(string path, CsvConfiguration config)
        {
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = ".";
            }

            var fileName = Path.GetFileName(path);

            var waitHandle = new AutoResetEvent(false);

            var fsw = new FileSystemWatcher(directory);

            fsw.Filter = fileName;
            fsw.EnableRaisingEvents = true;

            fsw.Changed += (s, e) =>
            {
                waitHandle.Set();
            };

            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    while (true)
                    {
                        var csvReader = new CsvReader(sr, config);
                        foreach (var record in csvReader.GetRecords<LogLine>())
                        {
                            yield return record;
                        }

                        waitHandle.WaitOne(1000);
                    }
                }
            }
            finally
            {
                waitHandle.Close();
            }
        }
    }
}
