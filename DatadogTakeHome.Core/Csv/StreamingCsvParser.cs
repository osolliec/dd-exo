using CsvHelper;
using CsvHelper.Configuration;
using DatadogTakeHome.Core.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DatadogTakeHome.Core.Csv
{
    /// <summary>
    /// The StreamingCsvParser is reponsible for reading csv files and output a IEnumerable of [LogLine].
    /// It will do so in a streaming fashion to avoid memory issues for large files.
    /// Internally, it uses the CsvHelper library. More information on https://joshclose.github.io/CsvHelper/.
    /// </summary>
    public class StreamingCsvParser : ICsvParser, IDisposable
    {
        
        private readonly CsvConfiguration _config;
        
        private CsvReader _csvReader;
        private StreamReader _streamReader;

        public StreamingCsvParser()
        {
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };
        }

        public void Dispose()
        {
            if (_streamReader != null)
            {
                _streamReader.Dispose();
            }
            if (_csvReader != null)
            {
                _csvReader.Dispose();
            }
        }

        public IEnumerable<LogLine> Parse(string path)
        {
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"File {path} not found");
            }

            _streamReader = new StreamReader(path);
            _csvReader = new CsvReader(_streamReader, _config);

            return _csvReader.GetRecords<LogLine>();
        }
    }
}
