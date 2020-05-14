using System.Collections.Generic;

namespace Venflow
{
    internal class DualKeyCollection<TKeyTwo, TValue> where TKeyTwo : notnull
                                                      where TValue : class
    {
        private readonly TValue[] _oneToValue;
        private readonly IDictionary<TKeyTwo, int> _twoToOne;

        internal int Count => _oneToValue.Length;

        internal ICollection<TKeyTwo> KeysTwo => _twoToOne.Keys;

        internal TValue[] Values => _oneToValue;

        internal DualKeyCollection(TValue[] firstCollction, Dictionary<TKeyTwo, int> twoToOne)
        {
            _oneToValue = firstCollction;
            _twoToOne = twoToOne;
        }

        internal TValue this[int key] => _oneToValue[key];

        internal TValue this[TKeyTwo key] => this[_twoToOne[key]];
    }
}
