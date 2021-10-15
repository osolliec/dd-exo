namespace DatadogTakeHome.Core.Model
{
    /// <summary>
    /// The request string, eg "POST /report HTTP/1.0", that has been split in multiple parts.
    /// </summary>
    public class ParsedRequest
    {
        public string HttpVerb { get; set; }
        public string Path { get; set; }
        public string Section { get; set; }
        public string HttpVersion { get; set; }
    }
}
