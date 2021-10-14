namespace DatadogTakeHome.Core.Model
{
    public class ParsedRequest
    {
        public string HttpVerb { get; set; }
        public string Path { get; set; }
        public string Section { get; set; }
        public string HttpVersion { get; set; }
    }
}
