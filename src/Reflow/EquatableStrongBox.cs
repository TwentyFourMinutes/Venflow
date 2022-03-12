namespace Reflow
{
    internal class EquatableStrongBox<T> : IEquatable<T> where T : struct, IEquatable<T>
    {
        private readonly T _value;

        public EquatableStrongBox(T value)
        {
            _value = value;
        }

        public bool Equals(T other)
        {
            return _value.Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}
