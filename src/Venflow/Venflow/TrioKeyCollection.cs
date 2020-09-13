using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Venflow
{

    internal class TrioKeyCollection<TKeyTwo, TKeyThree, TValue>
        where TKeyTwo : notnull
        where TKeyThree : notnull
        where TValue : class
    {
        private readonly TValue[] _oneToValue;
        private readonly IDictionary<TKeyTwo, TValue> _twoToOne;
        private readonly IDictionary<TKeyThree, TValue> _threeToOne;

        internal int Count => _oneToValue.Length;

        internal ICollection<TKeyTwo> KeysTwo => _twoToOne.Keys;
        internal ICollection<TKeyThree> KeysThree => _threeToOne.Keys;

        internal TValue[] Values => _oneToValue;

        internal TrioKeyCollection(TValue[] firstCollction, Dictionary<TKeyTwo, TValue> twoToOne, Dictionary<TKeyThree, TValue> threeToOne)
        {
            _oneToValue = firstCollction;
            _twoToOne = twoToOne;
            _threeToOne = threeToOne;
        }

        internal TValue this[int key] => _oneToValue[key];

        internal TValue this[TKeyTwo key] => _twoToOne[key];

        internal TValue this[TKeyThree key] => _threeToOne[key];

        internal bool TryGetValue(TKeyTwo key, [NotNullWhen(true)]out TValue? value)
        {
            return _twoToOne.TryGetValue(key, out value);
        }

        internal bool TryGetValue(TKeyThree key, [NotNullWhen(true)]out TValue? value)
        {
            return _threeToOne.TryGetValue(key, out value);
        }
    }
}
