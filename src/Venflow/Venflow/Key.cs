using System;
using System.ComponentModel;

namespace Venflow
{
    /// <summary>
    /// This is used to create strongly-typed ids.
    /// </summary>
    /// <typeparam name="T">They type of entity the key sits in.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>You can also create more specific implementations of this type, by creating a <i>struct</i> implementing <see cref="IKey{T, TKey}"/>.</remarks>
    public readonly struct Key<T, TKey> : IKey<T, TKey>, IEquatable<Key<T, TKey>>
            where TKey : struct, IEquatable<TKey>
    {
        private readonly TKey _value;

        TKey IKey<T, TKey>.Value { get => _value; }

        /// <summary>
        /// Instantiates a new <see cref="Key{T, TKey}"/> instance withe the provided value.
        /// </summary>
        /// <param name="value">The value which should represent the new <see cref="Key{T, TKey}"/> instance.</param>
        public Key(TKey value)
        {
            _value = value;
        }

        ///<inheritdoc/>
        public static implicit operator TKey(in Key<T, TKey> key)
        {
            return key._value;
        }

        ///<inheritdoc/>
        public static implicit operator Key<T, TKey>(in TKey value)
        {
            return new Key<T, TKey>(value);
        }

        ///<inheritdoc/>
        public static bool operator ==(in Key<T, TKey> a, in Key<T, TKey> b)
        {
            return a.Equals(b);
        }

        ///<inheritdoc/>
        public static bool operator !=(in Key<T, TKey> a, in Key<T, TKey> b)
        {
            return !a.Equals(b);
        }

        ///<inheritdoc/>
        public bool Equals(Key<T, TKey> other)
        {
            return other._value.Equals(this._value);
        }

        ///<inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is not Key<T, TKey> key)
            {
                return false;
            }

            return key._value.Equals(this._value);
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return _value.ToString();
        }
    }

    /// <summary>
    /// This interface should be implemented by <i>structs</i>, to create strongly-typed ids.
    /// </summary>
    /// <typeparam name="T">They type of entity the key sits in.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>See <see cref="Key{T, TKey}"/>for a possible implementation.</remarks>
    public interface IKey<T, TKey> : IKey
    {
        /// <summary>
        /// The underlying value representing the <see cref="Key{T, TKey}"/>.
        /// </summary>
        public TKey Value { get; }
    }

    /// <summary>
    /// <strong>Do not use this interface, if you are not absolutely sure what it does.</strong>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IKey
    {

    }
}
