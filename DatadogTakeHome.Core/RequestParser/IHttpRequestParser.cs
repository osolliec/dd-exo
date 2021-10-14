using DatadogTakeHome.Core.Model;

namespace DatadogTakeHome.Core.RequestParser
{
    public interface IHttpRequestParser
    {
        /// <summary>
        /// Parses a request into a ParsedRequest.
        /// </summary>
        /// <param name="s">A request string that looks like //GET /report HTTP/1.0</param>
        /// <returns></returns>
        ParsedRequest Parse(string s);
    }
}