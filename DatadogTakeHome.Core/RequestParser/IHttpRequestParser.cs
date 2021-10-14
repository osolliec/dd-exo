using DatadogTakeHome.Core.Model;

namespace DatadogTakeHome.Core.RequestParser
{
    public interface IHttpRequestParser
    {
        /// <summary>
        /// Try to parse a request into a ParsedRequest.
        /// </summary>
        /// <param name="s">A request string that looks like //GET /report HTTP/1.0</param>
        /// <param name="parsedRequest"></param>
        /// <returns>True if it could parse, false otherwise</returns>
        bool TryParse(string s, out ParsedRequest parsedRequest);
    }
}