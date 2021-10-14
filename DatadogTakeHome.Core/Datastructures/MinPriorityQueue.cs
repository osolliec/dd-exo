using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatadogTakeHome.Core.Datastructures
{
    /// <summary>
    /// C# doesn't ship its own PriorityQueue (yet: it should arrive in .NET CORE 6).
    /// I want to write one to reduce the complexity of the ordering of the section hits; instead of o(nlogn) by sorting the whole section list, we can go down to o(nlogk)
    ///
    /// It's based on a max binary heap, implemented with an array of a tuple (int, T), int being the priority of the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MinPriorityQueue<T>
    {
        private (long, T)[] _tree;
        private int _size = 0;
        private int _capacity;

        private static long _priorityFillValue => long.MaxValue;
        public MinPriorityQueue(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Size must be greater than 0");
            }
            _capacity = capacity;
            _tree = new (long, T)[capacity];
            Array.Fill(_tree, (_priorityFillValue, default(T)));
        }

        public int Size { get { return _size; } }

        public (long, T) Peek()
        {
            if (_size == 0)
                return (0, default(T));

            return _tree[0];
        }

        private static int Parent(int i)
        {
            return (i - 1) / 2;
        }

        private static int Left(int i)
        {
            return 2 * i + 1;
        }

        private static int Right(int i)
        {
            return 2 * i + 2;
        }

        private void Swap(int i1, int i2)
        {
            var temp = _tree[i1];
            _tree[i1] = _tree[i2];
            _tree[i2] = temp;
        }

        public void Insert(long priority, T value)
        {
            if (_size == _capacity)
            {
                throw new InvalidOperationException("full");
            }

            _tree[_size] = (priority, value);

            HeapifyUp(_size);

            _size++;
        }

        /// <summary>
        /// Foreach parent, if value is smaller than parent, swap value with parent
        /// </summary>
        /// <param name="index"></param>
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = Parent(index);
                if (_tree[index].Item1 < _tree[parent].Item1)
                {
                    Swap(index, parent);
                }

                index = parent;
            }
        }

        /// <summary>
        /// For each node, compare value with children. Take the smallest of the children, replace with current, continue with new children.
        /// Stop once value is smaller than both children.
        /// </summary>
        /// <param name="index"></param>
        private void HeapifyDown(int index)
        {
            if (index == _size - 1)
                return;

            int left = Left(index);
            int right = Right(index);

            int smaller = index;

            if (left < _size && _tree[left].Item1 <= _tree[right].Item1 && _tree[left].Item1 < _tree[index].Item1)
            {
                smaller = left;
            }
            if (right < _size && _tree[right].Item1 <= _tree[left].Item1 && _tree[right].Item1 < _tree[index].Item1)
            {
                smaller = right;
            }

            if (smaller != index)
            {
                Swap(smaller, index);
                HeapifyDown(smaller);
            }
        }


        public (long, T) ExtractMin()
        {
            (long, T) min = _tree[0];

            // put the rightmost element at the top
            Swap(0, _size - 1);

            // "empty" the rightmost element
            _tree[_size - 1] = (_priorityFillValue, default(T));

            _size--;

            HeapifyDown(0);

            return min;
        }
    }
}
