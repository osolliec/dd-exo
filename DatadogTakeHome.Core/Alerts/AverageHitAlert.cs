﻿using DatadogTakeHome.Core.Datastructures;
using DatadogTakeHome.Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatadogTakeHome.Core.Alerts
{
    /// <summary>
    /// This is the class that implements the guidelines of "Whenever total traffic for the past 2 minutes exceeds a certain number on average".
    ///
    /// It is configurable to allow changing the window size, and also the average hit threshold.
    /// </summary>
    public class AverageHitAlert : ILogAggregator
    {
        private readonly int _averageHitThreshold;
        private readonly int _windowDurationSeconds;

        private AlertStatus _status;

        public AlertStatus Status { get { return _status; } }

        /// <summary>
        /// Use a circular buffer to store the hits. It's convenient for representing a sliding window of time, each bucket represents a second.
        /// </summary>
        private CircularBuffer _buffer;

        /// <summary>
        /// An external message queue to publish messages.
        /// </summary>
        private Queue<string> _messageQueue;

        public enum AlertStatus
        {
            NOT_FIRING,
            FIRING,
        }

        /// <summary>
        /// Holds the first timestamp our program sees. It's necessary to not fire the alert until a first complete window of time has been seen.
        /// </summary>
        private long _startTimestamp = -1;

        /// <summary>
        /// The previous max timestamp seen so far.
        /// </summary>
        private long _previousMaxTimestamp = -1;

        /// <summary>
        /// Build the alert.
        /// </summary>
        /// <param name="windowDurationSeconds">The duration of the window for which we should keep the hit count.</param>
        /// <param name="averageThresholdPerSecond">The average threshold, for which, if exceeded for the duration of the window, the alert will fire a message.</param>
        public AverageHitAlert(int windowDurationSeconds, int averageThresholdPerSecond)
        {
            _averageHitThreshold = averageThresholdPerSecond;
            _windowDurationSeconds = windowDurationSeconds;

            _status = AlertStatus.NOT_FIRING;

            _buffer = new CircularBuffer(windowDurationSeconds);
        }

        /// <summary>
        /// Aggregates events that are not too late.
        ///
        /// Events that are rejected are the ones older than _previousMaxTimestamp - _windowDurationSeconds.
        /// </summary>
        /// <param name="logLine"></param>
        /// <param name="parsedRequest"></param>
        public void Collect(LogLine logLine, ParsedRequest parsedRequest)
        {
            // only take into account messages that are not too late
            if (logLine.TimestampSeconds <= _previousMaxTimestamp - _windowDurationSeconds)
            {
                return;
            }

            _buffer.Increment(logLine.TimestampSeconds);
        }

        /// <summary>
        /// Advance the time and change internal state of the alert.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        public void AdvanceTime(long maxTimestamp)
        {
            PossiblyChangeAlertState(maxTimestamp);
            CloseOldBuckets(maxTimestamp);
            UpdateTimestamps(maxTimestamp);
        }

        public void RegisterMessageQueue(Queue<string> messageQueue)
        {
            _messageQueue = messageQueue;
        }

        /// <summary>
        /// Possibly changes the alert state and write a message (FIRING/RESOLVED). Does not send the same message twice.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        private void PossiblyChangeAlertState(long maxTimestamp)
        {
            if (_status == AlertStatus.NOT_FIRING && ShouldFire(maxTimestamp))
            // The alert is firing for the first time since last status change
            {
                BuildMessage(true, maxTimestamp);
                _status = AlertStatus.FIRING;
            }
            else if (_status == AlertStatus.FIRING && !ShouldFire(maxTimestamp))
            // The alert is resolved for the first time since last status change
            {
                BuildMessage(false, maxTimestamp);
                _status = AlertStatus.NOT_FIRING;
            }
        }

        /// <summary>
        /// Returns true if we have gathered enough data and the average is above or equal to _averageHitThreshold.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        /// <returns></returns>
        private bool ShouldFire(long maxTimestamp)
        {
            bool firstWindowHasClosed = maxTimestamp >= _startTimestamp + _windowDurationSeconds;
            if (!firstWindowHasClosed)
            //Prevents the alert from firing when the first window hasn't closed yet - we don't have enough data.
            {
                return false;
            }

            return _buffer.GetAverage() >= _averageHitThreshold;
        }

        private void CloseOldBuckets(long newMaxTimestamp)
        {
            if (_previousMaxTimestamp == -1)
            {
                return;
            }

            // because we can handle low traffic websites where we don't see all timestamp increments, we can't just close the latest bucket.
            // We must close all buckets between the previous timestamp and the current one.
            _buffer.CloseBuckets(_previousMaxTimestamp + 1, newMaxTimestamp);
        }

        /// <summary>
        /// Update the internal timestamp trackers.
        /// </summary>
        /// <param name="maxTimestamp"></param>
        private void UpdateTimestamps(long maxTimestamp)
        {
            if (_startTimestamp == -1)
            {
                _startTimestamp = maxTimestamp;
            }

            _previousMaxTimestamp = maxTimestamp;
        }

        /// <summary>
        /// Build the message and append it to the registered message queue.
        /// </summary>
        /// <param name="firing"></param>
        /// <param name="triggerTime"></param>
        private void BuildMessage(bool firing, long triggerTime)
        {
            if (_messageQueue == null)
            {
                throw new InvalidOperationException("_messageQueue cannot be null. Please use the method RegisterMessageQueue");
            }

            var triggeredAt = DateFormatter.FormatDate(triggerTime);

            if (firing)
            {
                _messageQueue.Enqueue($"FIRING: High traffic generated an alert - total hits = {_buffer.GetTotal()} - on average = {_buffer.GetAverage()}, triggered at {triggeredAt}.");
            }
            else
            {
                _messageQueue.Enqueue($"RESOLVED: High traffic alert was resolved at {triggeredAt} - total hits = {_buffer.GetTotal()} - on average = {_buffer.GetAverage()}");
            }
        }
    }
}
