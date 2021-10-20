namespace Venflow
{
    internal class DualKeyCollection<TKeyTwo, TValue>
        where TKeyTwo : notnull
        where TValue : class
    {
        private readonly TValue[] _oneToValue;
        private readonly IDictionary<TKeyTwo, TValue> _twoToOne;

        internal int Count => _oneToValue.Length;

        internal TValue[] Values => _oneToValue;

        internal DualKeyCollection(TValue[] firstCollction, Dictionary<TKeyTwo, TValue> twoToOne)
        {
            _oneToValue = firstCollction;
            _twoToOne = twoToOne;
        }

        internal TValue this[int key] => _oneToValue[key];

        internal TValue this[TKeyTwo key] => _twoToOne[key];

        internal bool TryGetValue(TKeyTwo key, out TValue? value)
        {
            return _twoToOne.TryGetValue(key, out value);
        }
    }
}
