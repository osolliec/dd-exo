using DatadogTakeHome.Core.Model;
using System;

namespace DatadogTakeHome.Core.RequestParser
{
    public class HttpRequestParser : IHttpRequestParser
    {
        public ParsedRequest Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException("string is null or empty");
            }

            var splits = s.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length != 3)
            {
                throw new InvalidOperationException($"The request {s} cannot be parsed");
            }

            return new ParsedRequest()
            {
                HttpVerb = splits[0],
                Path = splits[1],
                Section = ParseSection(splits[1]),
                HttpVersion = splits[2],
            };
        }

        private string ParseSection(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException("string is null or empty");
            }

            if (!s.StartsWith("/"))
            {
                throw new ArgumentException($"String {s} does not seem to be a parsable path");
            }

            int indexOfSecondSlash = s.IndexOf("/", 1);

            if (indexOfSecondSlash == -1)
            {
                // the whole string is the section
                return s;
            }

            return s.Substring(0, indexOfSecondSlash);
        }
    }
}
