using DatadogTakeHome.Core.Datastructures;
using System;
using Xunit;

namespace DatadogTakeHome.Tests.UnitTests
{
    public class PriorityQueueTests
    {
        [Fact]
        public void WhenCreatingA0SizeQueue_PriorityQueue_ShouldThrow()
        {
            Assert.ThrowsAny<Exception>(() => new MinPriorityQueue<long>(0));
        }

        [Fact]
        public void WhenAddingMoreItemsThanCapacity_PriorityQueue_ShouldThrow()
        {
            var queue = new MinPriorityQueue<long>(2);

            queue.Insert(1, 1);
            queue.Insert(2, 2);

            Assert.ThrowsAny<Exception>(() => queue.Insert(3, 3));
        }


        [Fact]
        public void WhenAccessingPeek_PriorityQueue_ShouldReturnTheSmallestElement()
        {
            var queue = new MinPriorityQueue<long>(8);

            queue.Insert(100, 100);
            queue.Insert(90, 90);

            Assert.Equal(queue.Peek(), (90, 90));
        }

        [Fact]
        public void WhenInsertingASmallerElement_ExtractMin_ShouldReturnThisSmallerElement()
        {
            var queue = new MinPriorityQueue<long>(8);

            queue.Insert(100, 100);
            queue.Insert(1001, 1001);
            queue.Insert(1002, 1002);
            queue.Insert(10, 10);

            Assert.Equal(queue.Peek(), (10, 10));
            Assert.Equal(queue.ExtractMin(), (10, 10));
            Assert.Equal(queue.ExtractMin(), (100, 100));
            Assert.Equal(queue.ExtractMin(), (1001, 1001));
            Assert.Equal(queue.ExtractMin(), (1002, 1002));
        }
    }
}
