using System;
using System.Collections;
using System.Collections.Generic;

namespace FastCharts.Core.Utilities
{
    /// <summary>
    /// Read-only, non-copying view over a contiguous range of a list. Lets resamplers work on
    /// the visible slice of a large series without copying it.
    /// </summary>
    internal sealed class ListSegment<T> : IReadOnlyList<T>
    {
        private readonly IList<T> _source;
        private readonly int _offset;

        public ListSegment(IList<T> source, int offset, int count)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            if (offset < 0 || count < 0 || offset + count > source.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _offset = offset;
            Count = count;
        }

        public int Count { get; }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _source[_offset + index];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _source[_offset + i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
