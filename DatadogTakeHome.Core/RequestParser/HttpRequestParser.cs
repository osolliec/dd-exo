using DatadogTakeHome.Core.Model;
using System;

namespace DatadogTakeHome.Core.RequestParser
{
    public class HttpRequestParser : IHttpRequestParser
    {
        public bool TryParse(string s, out ParsedRequest parsedRequest)
        {
            parsedRequest = null;

            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            var splits = s.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (splits.Length != 3)
            {
                return false;
            }

            if (!TryParseSection(splits[1], out string section))
            {
                return false;
            }

            parsedRequest = new ParsedRequest()
            {
                HttpVerb = splits[0],
                Path = splits[1],
                Section = section,
                HttpVersion = splits[2],
            };

            return true;
        }

        private bool TryParseSection(string s, out string section)
        {
            section = null;
            if (string.IsNullOrWhiteSpace(s))
            {
                return false;
            }

            if (!s.StartsWith("/"))
            {
                return false;
            }

            int indexOfSecondSlash = s.IndexOf("/", 1);

            if (indexOfSecondSlash == -1)
            {
                // the whole string is the section
                section = s;
                return true;
            }

            section = s.Substring(0, indexOfSecondSlash);
            return true;
        }
    }
}
