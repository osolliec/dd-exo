using System;

namespace DatadogTakeHome.Core.Datastructures
{
    /// <summary>
    /// Circular buffer implemented with an array. The element before the first is the last, and the element after the last is the first.
    /// </summary>
    public class CircularBuffer
    {
        private readonly long[] _buffer;
        private long _total = 0;
        public CircularBuffer(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentException("Size must be greater than 0");
            }

            _buffer = new long[size];
        }

        private long Index(long key)
        {
            return key % _buffer.Length;
        }

        public void Increment(long key)
        {
            long index = Index(key);

            _buffer[index] = _buffer[index] + 1;

            _total++;
        }
        public void CloseBucket(long key)
        {
            long index = Index(key);
            _total = _total - _buffer[index];
            _buffer[index] = 0;
        }

        public void CloseBuckets(long from, long to)
        {
            for (long i = from; i <= to; i++)
            {
                CloseBucket(i);
            }
        }

        public long GetAverage()
        {
            return _total / _buffer.Length;
        }

        public long GetTotal()
        {
            return _total;
        }
    }
}
