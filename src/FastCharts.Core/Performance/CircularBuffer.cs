using System;

namespace FastCharts.Core.Performance
{
    /// <summary>
    /// Circular buffer for efficient storage of recent values
    /// </summary>
    internal class CircularBuffer<T>
    {
        private readonly T[] _buffer;
        private int _start;
        private int _size;
        private readonly int _capacity;

        public CircularBuffer(int capacity)
        {
            _capacity = capacity;
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            var end = (_start + _size) % _capacity;
            _buffer[end] = item;

            if (_size == _capacity)
            {
                _start = (_start + 1) % _capacity;
            }
            else
            {
                _size++;
            }
        }

        public void Clear()
        {
            _start = 0;
            _size = 0;
        }

        public int Count => _size;

        public T Latest => _size > 0 ? _buffer[(_start + _size - 1) % _capacity] : default(T)!;

        public double Average()
        {
            if (_size == 0 || typeof(T) != typeof(double)) return 0.0;

            double sum = 0;
            for (var i = 0; i < _size; i++)
            {
                var index = (_start + i) % _capacity;
                sum += Convert.ToDouble(_buffer[index]);
            }

            return sum / _size;
        }

        public double Maximum()
        {
            if (_size == 0 || typeof(T) != typeof(double)) return 0.0;

            var max = double.MinValue;
            for (var i = 0; i < _size; i++)
            {
                var index = (_start + i) % _capacity;
                var value = Convert.ToDouble(_buffer[index]);
                if (value > max) max = value;
            }

            return max;
        }
    }
}