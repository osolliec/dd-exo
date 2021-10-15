using DatadogTakeHome.Core.Csv;
using DatadogTakeHome.Core.Logger;
using DatadogTakeHome.Core.Model;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace DatadogTakeHome.Tests.UnitTests
{
    public class CsvParserTests
    {
        private StreamingCsvParser _parser;

        public CsvParserTests()
        {
            _parser = new StreamingCsvParser(new ConsoleLogger());
        }

        [Fact]
        public void WhenTheFileHasHeader_CsvParser_WillSuccessfullyParse()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"""remotehost"",""rfc931"",""authuser"",""date"",""request"",""status"",""bytes""" + Environment.NewLine);
            sb.Append(@"""10.0.0.2"",""-"",""apache"",1549573860,""GET / api / user HTTP / 1.0"",200,1234" + Environment.NewLine);

            var result = _parser.Parse(GetMemoryStream(sb)).ToList();

            Assert.Single(result);
            Assert.Equal(
                new LogLine()
                {
                    Authuser = "apache",
                    HttpStatusCode = 200,
                    RemoteHost = "10.0.0.2",
                    Request = "GET / api / user HTTP / 1.0",
                    RequestSizeBytes = 1234,
                    Rfc931 = "-",
                    TimestampSeconds = 1549573860
                },
                result[0]);
        }

        [Fact]
        public void WhenTheFileDoesNotHaveHeaders_CsvParser_WillParseSuccessFully()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"""10.0.0.2"",""-"",""apache"",1549573860,""GET / api / user HTTP / 1.0"",200,1234" + Environment.NewLine);

            var result = _parser.Parse(GetMemoryStream(sb)).ToList();

            Assert.Single(result);
            Assert.Equal(
                new LogLine()
                {
                    Authuser = "apache",
                    HttpStatusCode = 200,
                    RemoteHost = "10.0.0.2",
                    Request = "GET / api / user HTTP / 1.0",
                    RequestSizeBytes = 1234,
                    Rfc931 = "-",
                    TimestampSeconds = 1549573860
                },
                result[0]);
        }

        [Fact]
        public void WhenTheFileHasRecordsWithMissingField_CsvParser_WillIgnoreTheBadRecords()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"""remotehost"",""rfc931"",""authuser"",""date"",""request"",""status"",""bytes""" + Environment.NewLine);

            // wrongly formed record 1 with missing size field
            sb.Append(@"""10.0.0.2"",""-"",142,1549573860,""GET / api / user HTTP / 1.0"",200" + Environment.NewLine);

            // wrongly formed record 2 with text instead of number
            sb.Append(@"""10.0.0.2"",""-"",""apache"",1549573860,""GET / api / user HTTP / 1.0"",200,""NOTANUMBER""" + Environment.NewLine);

            // good record
            sb.Append(@"""10.0.0.2"",""-"",""apache"",1549573860,""GET / api / user HTTP / 1.0"",200,1234" + Environment.NewLine);

            var result = _parser.Parse(GetMemoryStream(sb)).ToList();

            Assert.Single(result);
            Assert.Equal(
                new LogLine()
                {
                    Authuser = "apache",
                    HttpStatusCode = 200,
                    RemoteHost = "10.0.0.2",
                    Request = "GET / api / user HTTP / 1.0",
                    RequestSizeBytes = 1234,
                    Rfc931 = "-",
                    TimestampSeconds = 1549573860
                },
                result[0]);
        }

        private MemoryStream GetMemoryStream(StringBuilder sb)
        {
            var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes(sb.ToString()));
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public void Dispose()
        {
            _parser.Dispose();
        }
    }
}
