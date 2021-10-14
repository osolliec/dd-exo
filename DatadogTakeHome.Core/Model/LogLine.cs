using CsvHelper.Configuration.Attributes;
using System;

namespace DatadogTakeHome.Core.Model
{
    /// <summary>
    /// Represents a csv stream line.
    /// </summary>
    public class LogLine
    {
        /// <summary>
        /// It's the IP address of the client making the request: eg 10.0.0.2
        /// </summary>
        [Name("remotehost")]
        public string RemoteHost { get; set; }
        [Name("rfc931")]
        public string Rfc931 { get; set; }
        [Name("authuser")]
        public string Authuser { get; set; }
        /// <summary>
        /// The epoch timestamp in seconds. Might as well use long to not hit the 2038 bug (hitting the max int value)
        /// </summary>
        [Name("date")]
        public long TimestampSeconds { get; set; }
        /// <summary>
        /// A string that contains HTTP VERB - relative path - HTPP VERSION. Eg : "POST /report HTTP/1.0"
        /// </summary>
        [Name("request")]
        public string Request { get; set; }
        /// <summary>
        /// Http Status Code (eg: 201)
        /// </summary>
        [Name("status")]
        public int HttpStatusCode { get; set; }
        /// <summary>
        /// The size of the request in bytes.
        /// </summary>
        [Name("bytes")]
        public int RequestSizeBytes { get; set; }

        public override bool Equals(object obj)
        {
            return obj is LogLine line &&
                   RemoteHost == line.RemoteHost &&
                   Rfc931 == line.Rfc931 &&
                   Authuser == line.Authuser &&
                   TimestampSeconds == line.TimestampSeconds &&
                   Request == line.Request &&
                   HttpStatusCode == line.HttpStatusCode &&
                   RequestSizeBytes == line.RequestSizeBytes;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RemoteHost, Rfc931, Authuser, TimestampSeconds, Request, HttpStatusCode, RequestSizeBytes);
        }
    }
}
