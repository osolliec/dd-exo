using DatadogTakeHome.Core.Model;
using DatadogTakeHome.Core.Stats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xunit;

namespace DatadogTakeHome.Tests.UnitTests
{
    /** In the real world, I would not test result against the message string which may change often. I did this for the sake of simplicity in this file. **/
    public class PeriodicSummaryReportTests
    {
        [Fact]
        public void WhenWindowHasClosed_TheReport_ShouldHoldAMessage()
        {
            var report = BuildReport(out var messageQueue);
            var logLine = BuildLogLine(1);
            var parsedRequest = BuildParsedRequest();

            report.AdvanceTime(1);

            report.Collect(logLine, parsedRequest);
            report.Collect(logLine, parsedRequest);

            report.AdvanceTime(3);

            Assert.Single(messageQueue);
        }

        [Fact]
        public void WhenReceivingLateEventsThatExceedWindowTime_TheReport_ShouldIgnoreThoseLateEvents()
        {
            var report = BuildReport(out var messageQueue);
            var parsedRequest = BuildParsedRequest();

            report.AdvanceTime(1);

            // valid events
            report.Collect(BuildLogLine(1), parsedRequest);
            report.Collect(BuildLogLine(2), parsedRequest);

            report.AdvanceTime(3);

            // lateEvent, arrived after the bucket has closed.
            report.Collect(BuildLogLine(1), parsedRequest);

            Assert.Contains("TOTAL HITS: 2", messageQueue.Dequeue());
        }

        [Fact]
        public void WhenReceivingEventsLaterThanCurrentTimestamp_TheReport_ShouldAcceptThoseEvents()
        {
            var report = BuildReport(out var messageQueue);
            var parsedRequest = BuildParsedRequest();

            report.AdvanceTime(1);

            report.Collect(BuildLogLine(1), parsedRequest);

            report.AdvanceTime(2);

            // old event relative to max timestamp seen, but it's still valid
            report.Collect(BuildLogLine(1), parsedRequest);

            report.AdvanceTime(3);

            Assert.Contains("TOTAL HITS: 2", messageQueue.Dequeue());
        }

        /// <summary>
        /// This can happen when the traffic is low and we don't have every timestamp in the logs.
        /// </summary>
        [Fact]
        public void WhenMessagesDontHaveConsecutiveTimestamps_TheReport_ShouldStillFireAWindowEvenThoughItsBiggerThanRequested()
        {
            var report = BuildReport(out var messageQueue);
            var parsedRequest = BuildParsedRequest();

            report.AdvanceTime(1);

            // valid events
            report.Collect(BuildLogLine(1), parsedRequest);

            // advance time to close the window
            report.AdvanceTime(12);

            // this event won't be collected in the current message
            report.Collect(BuildLogLine(12), parsedRequest);

            var message = messageQueue.Dequeue();

            // the window is bigger than requested size, but it's by design.
            Assert.Contains("REPORT - FROM 1970-01-01 00:00:01Z TO 1970-01-01 00:00:11Z", message);
            Assert.Contains("TOTAL HITS: 1", message);
        }

        [Fact]
        public void WhenThereAreNoHitsOnCommonStatusCodes_TheReport_ShouldStillDisplayThem()
        {
            var report = BuildReport(out var messageQueue, 1);
            var parsedRequest = BuildParsedRequest();

            report.AdvanceTime(1);

            report.Collect(BuildLogLine(1, 111), parsedRequest);
            report.Collect(BuildLogLine(1, 111), parsedRequest);

            report.AdvanceTime(2);

            var message = messageQueue.Dequeue();

            Assert.Contains("HTTP_CODE: 200 HITS: 0", message);
            Assert.Contains("HTTP_CODE: 300 HITS: 0", message);
            Assert.Contains("HTTP_CODE: 400 HITS: 0", message);
            Assert.Contains("HTTP_CODE: 500 HITS: 0", message);
        }

        [Fact]
        public void WhenThereAreHitsOnUncommonStatusCodes_TheReport_ShouldDisplayThem()
        {
            var report = BuildReport(out var messageQueue);

            report.AdvanceTime(1);

            report.Collect(BuildLogLine(1, 111), BuildParsedRequest("/api"));
            report.Collect(BuildLogLine(1, 112), BuildParsedRequest("/user"));

            report.AdvanceTime(3);

            var message = messageQueue.Dequeue();

            Assert.Contains("HTTP_CODE: 111 HITS: 1", message);
            Assert.Contains("HTTP_CODE: 112 HITS: 1", message);
        }

        [Fact]
        public void WhenThereAreHitsOnSections_TheReport_ShouldDisplayTheSectionHits()
        {
            var report = BuildReport(out var messageQueue);

            report.AdvanceTime(1);

            report.Collect(BuildLogLine(1), BuildParsedRequest("/api"));
            report.Collect(BuildLogLine(1), BuildParsedRequest("/user"));

            report.AdvanceTime(3);

            var message = messageQueue.Dequeue();

            Assert.Contains("SECTION: /api HITS: 1", message);
            Assert.Contains("SECTION: /user HITS: 1", message);
        }

        [Fact]
        public void WhenThereAreMoreThan5Sections_TheReport_ShouldKeepTheTop5SectionHits()
        {
            var report = BuildReport(out var messageQueue);

            report.AdvanceTime(1);

            // Add 20 different sections
            for (int i = 1; i <= 20; i++)
            {
                for (int j = 0; j < i; j ++)
                {
                    report.Collect(BuildLogLine(1), BuildParsedRequest($"/api-{i}"));
                }
            }

            report.AdvanceTime(3);

            Assert.True(StringEqualWithoutSpace(
                @"REPORT - FROM 1970-01-01 00:00:01Z TO 1970-01-01 00:00:02Z
                TOTAL HITS: 210 
                TOP 5 SECTIONS HITS: 
                SECTION: /api-20 HITS: 20 
                SECTION: /api-19 HITS: 19 
                SECTION: /api-18 HITS: 18 
                SECTION: /api-17 HITS: 17 
                SECTION: /api-16 HITS: 16 
                HTTP_CODE: 200 HITS: 210 
                HTTP_CODE: 300 HITS: 0 
                HTTP_CODE: 400 HITS: 0 
                HTTP_CODE: 500 HITS: 0",
                messageQueue.Dequeue()
                ));
        }




        [Fact]
        public void WhenWindowHasClosed_TheReport_ShouldDisplayAllHits()
        {
            var report = BuildReport(out var messageQueue);
            var parsedRequest = BuildParsedRequest();

            report.AdvanceTime(1);

            report.Collect(BuildLogLine(1, 404), parsedRequest);
            report.Collect(BuildLogLine(1, 503), parsedRequest);

            report.AdvanceTime(3);
            Assert.True(
                StringEqualWithoutSpace(
                    @"REPORT - FROM 1970-01-01 00:00:01Z TO 1970-01-01 00:00:02Z
                    TOTAL HITS: 2 
                    TOP 5 SECTIONS HITS: 
                    SECTION: /api HITS: 2 
                    HTTP_CODE: 200 HITS: 0 
                    HTTP_CODE: 300 HITS: 0 
                    HTTP_CODE: 400 HITS: 0 
                    HTTP_CODE: 404 HITS: 1 
                    HTTP_CODE: 500 HITS: 0 
                    HTTP_CODE: 503 HITS: 1",
                    messageQueue.Dequeue()
                    )
                );
        }

        private PeriodicSummaryReport BuildReport(out Queue<string> messageQueue, int windowSize = 2)
        {
            var report = new PeriodicSummaryReport(windowSize);
            messageQueue = new Queue<string>();
            report.RegisterMessageQueue(messageQueue);
            return report;
        }

        private LogLine BuildLogLine(long timestamp = 0, int httpCode = 200)
        {
            return new LogLine()
            {
                Authuser = "-",
                HttpStatusCode = httpCode,
                RemoteHost = "-",
                Request = "-",
                RequestSizeBytes = 0,
                Rfc931 = "-",
                TimestampSeconds = timestamp
            };
        }

        private ParsedRequest BuildParsedRequest(string section = "/api")
        {
            return new ParsedRequest()
            {
                HttpVerb = "GET",
                HttpVersion = "HTTP/1.0",
                Path = "/api/help",
                Section = section
            };
        }

        /// <summary>
        /// Compare the equality of the two strings without the space.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private bool StringEqualWithoutSpace(string s1, string s2)
        {
            string normalized1 = Regex.Replace(s1, @"\s", "");
            string normalized2 = Regex.Replace(s2, @"\s", "");

            return string.Equals(
                normalized1,
                normalized2
            );
        }
    }
}
