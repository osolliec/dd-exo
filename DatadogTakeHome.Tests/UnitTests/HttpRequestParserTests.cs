using DatadogTakeHome.Core.RequestParser;
using System;
using Xunit;

namespace DatadogTakeHome.Tests.UnitTests
{
    public class HttpRequestParserTests
    {
        private HttpRequestParser BuildParser()
        {
            return new HttpRequestParser();
        }

        [Fact]
        public void WhenRequestHasTwoSpaces_HttpRequestParser_ShouldSuccessfullyParseTheRequest()
        {
            var parser = BuildParser();

            parser.TryParse("GET /api/help HTTP/1.0", out var request);

            Assert.Equal("GET", request.HttpVerb);
            Assert.Equal("/api/help", request.Path);
            Assert.Equal("/api", request.Section);
            Assert.Equal("HTTP/1.0", request.HttpVersion);
        }

        [Fact]
        public void WhenSectionIsQuiteLong_HttpRequestParser_ShouldSuccessfullyParseTheSection()
        {
            var parser = BuildParser();

            parser.TryParse("GET /api/help/a/very/long/path/indeed HTTP/1.0", out var request);

            Assert.Equal("/api", request.Section);
        }

        [Fact]
        public void WhenSectionIsEquivalentToPath_HttpRequestParser_ShouldSuccessfullyParseTheSection()
        {
            var parser = BuildParser();

            parser.TryParse("GET /api HTTP/1.0", out var request);

            Assert.Equal("/api", request.Section);
        }

        [Fact]
        public void WhenStringIsNull_HttpRequestParser_ShouldNotParseTheRequest()
        {
            var parser = BuildParser();

            Assert.False(parser.TryParse(null, out var section));
        }

        [Fact]
        public void WhenStringIsEmpty_HttpRequestParser_ShouldNotParseTheRequest()
        {
            var parser = BuildParser();

            Assert.False(parser.TryParse("", out var section));
        }

        [Fact]
        public void WhenStringIsWhiteSpace_HttpRequestParser_ShouldNotParseTheRequest()
        {
            var parser = BuildParser();

            Assert.False(parser.TryParse(" ", out var section));
        }

        [Fact]
        public void WhenRequestHasMissingHttpInfo_HttpRequestParser_ShouldNotParseTheRequest()
        {
            var parser = BuildParser();

            Assert.False(parser.TryParse("GET /api/help/no/http/info", out var section));
        }

        [Fact]
        public void WhenRequestHasMissingHttpInfoButWithATrailingSpace_HttpRequestParser_ShouldNotParseTheRequest()
        {
            var parser = BuildParser();

            Assert.False(parser.TryParse("GET /api/help/no/http/info ", out var section));
        }

        [Fact]
        public void WhenRequestHasMissingPrefixSlash_HttpRequestParser_ShouldNotParseTheRequest()
        {
            var parser = BuildParser();

            Assert.False(parser.TryParse("GET i_forgot_the_trailing_slash_here HTTP/1.0", out var section));
        }
    }
}
