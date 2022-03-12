using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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

        public override int GetHashCode()
        {
            if (_value is null)
                return 0;

            return _value.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (_value is null)
            {
                return obj is null;
            }

            return _value.Equals(obj);
        }
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

        public override bool Equals(object? obj) => Equals(obj as IInterpolationArgument);
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

        public override bool Equals(object? obj) => Equals(obj as IInterpolationArgument);
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

        public override bool Equals(object? obj) => Equals(obj as IInterpolationArgument);
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

    internal class QueryCacheKey : IEquatable<QueryCacheKey>
    {
        private readonly MethodInfo _methodInfo;
        private readonly IList<IInterpolationArgument> _arguments;

        internal QueryCacheKey(MethodInfo methodInfo, IList<IInterpolationArgument> arguments)
        {
            _methodInfo = methodInfo;
            _arguments = arguments;
        }

        public bool Equals(QueryCacheKey? other)
        {
            if (
                other is null
                || _methodInfo != other._methodInfo
                || _arguments.Count != other._arguments.Count
            )
            {
                return false;
            }

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
            var hash = new HashCode();

            hash.Add(_methodInfo.GetHashCode());

            if (_arguments is not null)
            {
                hash.Add(
                    ((IStructuralEquatable)_arguments).GetHashCode(
                        InterpolationArgumentEqualityComparer.Default
                    )
                );
            }

            return hash.ToHashCode();
        }

        public override bool Equals(object? obj) => Equals(obj as QueryCacheKey);
    }
}
