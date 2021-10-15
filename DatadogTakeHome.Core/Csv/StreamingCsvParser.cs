using CsvHelper;
using CsvHelper.Configuration;
using DatadogTakeHome.Core.Logger;
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
    /// It will ignore badly formed rows.
    /// Internally, it uses the CsvHelper library. More information on https://joshclose.github.io/CsvHelper/.
    /// </summary>
    public class StreamingCsvParser : ICsvParser, IDisposable
    {
        private readonly ILogger _logger;
        private readonly CsvConfiguration _config;

        private CsvReader _csvReader;
        private StreamReader _streamReader;

        public StreamingCsvParser(ILogger logger)
        {
            _logger = logger;
            _config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Pretend the file does not have a header.
                // Even though the sample has a header, it's easier for the tailer if we skip it altogether
                // The first row will be a "bad record" and will be ignored.
                HasHeaderRecord = false,
                ReadingExceptionOccurred = OnReadingException,
                BadDataFound = OnBadDataFound,
                MissingFieldFound = OnMissingField
            };
        }

        private void OnMissingField(MissingFieldFoundArgs args)
        {
            // same as below, necessary to declare but we don't need to do anything here.
        }

        private void OnBadDataFound(BadDataFoundArgs args)
        {
            // gets called when a field is badly formed (string instead of number).
            // We need it even though we don't use it otherwise the OnReadingException below does not get called.
        }

        private bool OnReadingException(ReadingExceptionOccurredArgs args)
        {
            var rawRecord = args.Exception?.Context?.Parser?.RawRecord;
            if (rawRecord != null && rawRecord.StartsWith("\"remotehost\""))
            {
                // Ignore the header record.
                return false;
            }

            _logger.Log(LogLevel.Error, null, $"Found bad csv record at row {args.Exception?.Context?.Parser?.Row}: {rawRecord}. Ignoring");
            _logger.Log(LogLevel.Error, args.Exception, "");

            // prevent the exception from throwing; here we could count the errors and raise an alert if it exceeds a threshol.
            return false;
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

            var tailer = new FileTailer();

            return tailer.ContinuouslyReadFile(path, _config);
        }

        public IEnumerable<LogLine> Parse(Stream stream)
        {
            _streamReader = new StreamReader(stream);
            _csvReader = new CsvReader(_streamReader, _config);
            return _csvReader.GetRecords<LogLine>();
        }
    }
}
