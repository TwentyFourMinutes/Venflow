using System.Collections;
using System.Runtime.CompilerServices;

namespace Reflow.Analyzer
{
    internal class IndexedStack<T> : IReadOnlyList<T>
    {
        public int Count { get; private set; }

        private T[] _array;
        private int _size;

        private const int DefaultCapacity = 4;

        internal IndexedStack()
        {
            _array = Array.Empty<T>();
        }

        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public void Push(T item)
        {
            var size = _size;
            var array = _array;

            if ((uint)size < (uint)array.Length)
            {
                array[size] = item;
                _size = size + 1;
            }
            else
            {
                PushWithResize(item);
            }

            Count++;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void PushWithResize(T item)
        {
            Grow(_size + 1);
            _array[_size] = item;
            _size++;
        }

        private void Grow(int capacity)
        {
            var newcapacity = _array.Length == 0 ? DefaultCapacity : 2 * _array.Length;

            if ((uint)newcapacity > 0X7FFFFFC7)
                newcapacity = 0X7FFFFFC7;

            if (newcapacity < capacity)
                newcapacity = capacity;

            Array.Resize(ref _array, newcapacity);
        }

        public T Pop()
        {
            var size = _size - 1;
            var array = _array;

            if ((uint)size >= (uint)array.Length)
            {
                throw new IndexOutOfRangeException();
            }

            _size = size;
            var item = array[size];

            array[size] = default!;

            Count--;

            return item;
        }

        public IEnumerator<T> GetEnumerator() => ((IReadOnlyList<T>)_array).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _array.GetEnumerator();
    }
}
