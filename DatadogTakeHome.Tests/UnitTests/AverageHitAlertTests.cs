using DatadogTakeHome.Core.Alerts;
using DatadogTakeHome.Core.Model;
using System.Collections.Generic;
using Xunit;

namespace DatadogTakeHome.Tests.UnitTests
{
    public class AverageHitAlertTests
    {
        [Fact]
        public void WhenThereIsNotEnoughDataButThresholdIsSurpassed_TheAlert_ShouldNotFire()
        {
            var alert = BuildEmptyAlert(out var messageQueue);

            alert.AdvanceTime(1);

            for (int i = 0; i < 10; i++)
            {
                alert.Collect(BuildLogLine(1), BuildParsedRequest());
            }

            alert.AdvanceTime(2);

            // the average for this size 2 alert is 5, which is above the trigger. We still don't fire the alert because not enough time has passed.
            Assert.Empty(messageQueue);
        }

        [Fact]
        public void WhenEnoughDataHasBeenGatheredAndThresholdIsSurpassed_TheAlert_ShouldFire()
        {
            var alert = BuildEmptyAlert(out var messageQueue);

            alert.AdvanceTime(1);

            for (int i = 0; i < 10; i++)
            {
                alert.Collect(BuildLogLine(i % 5), BuildParsedRequest());
            }

            // we see for the first time the timestamp 3
            alert.AdvanceTime(3);

            // the average for this size 2 alert is 5 (10 events / 2).
            Assert.Equal(AverageHitAlert.AlertStatus.FIRING, alert.Status);
            Assert.Single(messageQueue);
            Assert.Equal("FIRING: High traffic generated an alert - total hits = 10 - on average = 5, triggered at 1970-01-01 00:00:03Z.", messageQueue.Dequeue());
        }

        [Fact]
        public void WhenEnoughDataHasBeenGatheredButThresholdIsNOTSurpassed_TheAlert_ShouldNOTFire()
        {
            var alert = BuildEmptyAlert(out var messageQueue);

            alert.AdvanceTime(1);

            alert.Collect(BuildLogLine(1), BuildParsedRequest());
            alert.Collect(BuildLogLine(2), BuildParsedRequest());

            alert.AdvanceTime(3);

            Assert.Equal(AverageHitAlert.AlertStatus.NOT_FIRING, alert.Status);
            Assert.Empty(messageQueue);
        }

        [Fact]
        public void WhenNotResolved_TheAlert_ShouldNotFireASecondMessage()
        {
            // the alert is already firing
            var alert = BuildFiringAlert(out var messageQueue);

            for (int i = 0; i < 10; i++)
            {
                // increase the average value for the hits
                alert.Collect(BuildLogLine(3), BuildParsedRequest());
            }

            alert.AdvanceTime(3);

            Assert.Equal(AverageHitAlert.AlertStatus.FIRING, alert.Status);
            // the alert's message won't be sent a second time - even though the alert is still firing.
            Assert.Single(messageQueue);
        }

        [Fact]
        public void WhenResolved_TheAlert_ShouldSendAMessage()
        {
            var alert = BuildFiringAlert(out var messageQueue);

            // empty the firing message
            messageQueue.Dequeue();

            // pass the time without collecting any new data to resolve the alert
            alert.AdvanceTime(10);

            Assert.Equal(AverageHitAlert.AlertStatus.NOT_FIRING, alert.Status);
            Assert.Single(messageQueue);
            Assert.Equal("RESOLVED: High traffic alert was resolved at 1970-01-01 00:00:10Z - total hits = 0 - on average = 0", messageQueue.Dequeue());
        }

        [Fact]
        public void WhenLateEventsDontExceedWindowSize_TheAlert_WillAcceptThoseLateEvents()
        {
            var alert = BuildEmptyAlert(out var messageQueue);

            alert.AdvanceTime(1);
            alert.AdvanceTime(3);

            for (int i = 0; i < 4; i++)
            {
                // collect events of timestamp 2, even though we've already seen timestamp 3.
                // it's collected because the window is of size 2.
                alert.Collect(BuildLogLine(2), BuildParsedRequest());
            }

            // trigger the alert
            alert.AdvanceTime(4);

            Assert.Equal(AverageHitAlert.AlertStatus.FIRING, alert.Status);
            Assert.Single(messageQueue);
            Assert.Equal("FIRING: High traffic generated an alert - total hits = 4 - on average = 2, triggered at 1970-01-01 00:00:04Z.", messageQueue.Dequeue());
        }

        [Fact]
        public void WhenReceivingLateEventsThatExceedWindowSize_TheAlert_ShouldNotAcceptThoseLateEvents()
        {
            var alert = BuildEmptyAlert(out var messageQueue);

            alert.AdvanceTime(1);
            alert.AdvanceTime(3);

            for (int i = 0; i < 4; i++)
            {
                // those events should not be collected, because they would write in the same slot as the events of timestamp 3.
                alert.Collect(BuildLogLine(1), BuildParsedRequest());
            }

            // possibly trigger the alert
            alert.AdvanceTime(4);

            Assert.Equal(AverageHitAlert.AlertStatus.NOT_FIRING, alert.Status);
            Assert.Empty(messageQueue);
        }


        /// <summary>
        /// Build an alert that is firing and has a message to collect.
        /// </summary>
        /// <returns></returns>
        private AverageHitAlert BuildFiringAlert(out Queue<string> messageQueue)
        {
            messageQueue = new Queue<string>();

            var alert = new AverageHitAlert(2, 2);

            alert.RegisterMessageQueue(messageQueue);

            alert.AdvanceTime(1);

            for (int i = 0; i < 10; i++)
            {
                alert.Collect(BuildLogLine(1), BuildParsedRequest());
            }

            alert.AdvanceTime(3);

            return alert;
        }

        /// <summary>
        /// Build an alert that is firing and has a message to collect.
        /// </summary>
        /// <returns></returns>
        private AverageHitAlert BuildEmptyAlert(out Queue<string> messageQueue)
        {
            messageQueue = new Queue<string>();

            var alert = new AverageHitAlert(2, 2);

            alert.RegisterMessageQueue(messageQueue);

            return alert;
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
    }
}
