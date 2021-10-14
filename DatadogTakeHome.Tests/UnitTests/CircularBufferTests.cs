using DatadogTakeHome.Core.Datastructures;
using System;
using Xunit;

namespace DatadogTakeHome.Tests.UnitTests
{
    public class CircularBufferTests
    {
        [Fact]
        public void WhenCreatingA0SizeBuffer_CircularBuffer_ShouldThrow()
        {
            Assert.ThrowsAny<Exception>(() => new CircularBuffer(0));
        }

        [Fact]
        public void WhenIncrementing_CircularBuffer_ShouldComputeAverageAndTotal()
        {
            var buffer = new CircularBuffer(2);

            buffer.Increment(0);
            buffer.Increment(1);

            Assert.Equal(2, buffer.GetTotal());
            Assert.Equal(1, buffer.GetAverage());
        }

        [Fact]
        public void WhenClosingABucket_TheBucketValue_ShouldBeSubstractedFromTheTotal()
        {
            var buffer = new CircularBuffer(2);

            buffer.Increment(0);

            for (int i = 0; i < 10; i++)
            {
                buffer.Increment(1);
            }

            buffer.CloseBucket(1);

            Assert.Equal(1, buffer.GetTotal());
            Assert.Equal(0, buffer.GetAverage());
        }
    }
}
