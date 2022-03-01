using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Reflow
{
    internal interface IInterpolationArgument : IEquatable<IInterpolationArgument>
    {
        int GetHashCode();
    }

    internal class InterpolationArgument : IInterpolationArgument
    {
        private readonly object _value;

        internal InterpolationArgument(object value)
        {
            _value = value;
        }

        public bool Equals(IInterpolationArgument? other)
        {
            if (other is not InterpolationArgument { _value: var otherValue })
                return false;

            return _value!.Equals(otherValue);
        }

        public override int GetHashCode() => _value!.GetHashCode();
    }

    internal class InterpolationArgument<T> : IInterpolationArgument where T : struct, IEquatable<T>
    {
        private readonly T _value;

        internal InterpolationArgument(T value)
        {
            _value = value;
        }

        public bool Equals(IInterpolationArgument? other)
        {
            if (other is not InterpolationArgument<T> { _value: var otherValue })
                return false;

            return _value!.Equals(otherValue);
        }

        public override int GetHashCode() => _value!.GetHashCode();
    }

    internal class InterpolationListArgument : IInterpolationArgument
    {
        private readonly IList<object> _value;

        internal InterpolationListArgument(IList<object> value)
        {
            _value = value;
        }

        public bool Equals(IInterpolationArgument? other)
        {
            if (
                other is not InterpolationListArgument { _value: var otherValue }
                || _value.Count != otherValue.Count
            )
            {
                return false;
            }

            for (var index = 0; index < _value.Count; index++)
            {
                if (_value[index].Equals(otherValue[index]))
                    return false;
            }

            return _value.Equals(otherValue);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<object>.Default);
        }
    }

    internal class InterpolationListArgument<T> : IInterpolationArgument where T : IEquatable<T>
    {
        private readonly IList<T> _value;

        internal InterpolationListArgument(IList<T> value)
        {
            _value = value;
        }

        public bool Equals(IInterpolationArgument? other)
        {
            if (
                other is not InterpolationListArgument<T> { _value: var otherValue }
                || _value.Count != otherValue.Count
            )
            {
                return false;
            }

            for (var index = 0; index < _value.Count; index++)
            {
                if (_value[index].Equals(otherValue[index]))
                    return false;
            }

            return _value.Equals(otherValue);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)_value).GetHashCode(EqualityComparer<T>.Default);
        }
    }

    internal class InterpolationArgumentEqualityComparer
        : IEqualityComparer<IInterpolationArgument>,
          IEqualityComparer
    {
        public static InterpolationArgumentEqualityComparer Default { get; } = new();

        public bool Equals(IInterpolationArgument? x, IInterpolationArgument? y)
        {
            if (x is null || y is null)
                return false;

            return x.Equals(y);
        }

        public new bool Equals(object? x, object? y)
        {
            if (
                x is not IInterpolationArgument xArgument
                || y is not IInterpolationArgument yArgument
            )
                return false;

            return xArgument.Equals(yArgument);
        }

        public int GetHashCode([DisallowNull] IInterpolationArgument obj)
        {
            return obj.GetHashCode();
        }

        public int GetHashCode(object obj)
        {
            return obj.GetHashCode();
        }
    }

    internal class InterpolationArgumentCollection : IEquatable<InterpolationArgumentCollection>
    {
        private readonly IList<IInterpolationArgument> _arguments;

        internal InterpolationArgumentCollection(IList<IInterpolationArgument> arguments)
        {
            _arguments = arguments;
        }

        public bool Equals(InterpolationArgumentCollection? other)
        {
            if (other is null || _arguments.Count != other._arguments.Count)
                return false;

            for (var argumentIndex = 0; argumentIndex < _arguments.Count; argumentIndex++)
            {
                if (!_arguments[argumentIndex].Equals(other._arguments[argumentIndex]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable)_arguments).GetHashCode(
                InterpolationArgumentEqualityComparer.Default
            );
        }
    }
}
