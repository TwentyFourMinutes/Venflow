using System.Runtime.CompilerServices;
using System.Text;
using Npgsql;

namespace Reflow.Operations
{
    [InterpolatedStringHandler]
    public struct SqlInterpolationHandler
    {
        internal class AmbientData
        {
            [ThreadStatic]
            internal static AmbientData? Current;

            internal StringBuilder CommandBuilder = null!;
            internal NpgsqlParameterCollection Parameters = null!;
            internal short[] ParameterIndecies = null!;
            internal string[] HelperStrings = null!;

            internal bool Caching = false;
            internal IInterpolationArgument[] Arguments = null!;
        }

        private int _interpolationIndex;
        private short _parameterIndex;
        private short _absolutParameterIndex;
        private short _nextParameterIndex;
        private int _helperStringsIndex;

        private readonly StringBuilder _commandBuilder;
        private readonly short[]? _parameterIndecies;
        private readonly string[]? _helperStrings;
        private readonly NpgsqlParameterCollection _parameters;

        private readonly bool _caching;
        private readonly IInterpolationArgument[] _arguments;
        private int _argumentIndex;

        public SqlInterpolationHandler(int literalLength, int formattedCount)
        {
            _ = literalLength;
            _ = formattedCount;

            _interpolationIndex = 0;
            _parameterIndex = 0;
            _absolutParameterIndex = 0;
            _helperStringsIndex = 0;

            var current = AmbientData.Current;

            if (current is null)
                throw new InvalidOperationException(
                    $"Invalid usage of the '{nameof(SqlInterpolationHandler)}' struct, it should never be called explicitly by user code."
                );

            _caching = current.Caching;
            _parameterIndecies = current.ParameterIndecies;
            _helperStrings = current.HelperStrings;
            _nextParameterIndex =
                _parameterIndecies?.Length > 0 ? _parameterIndecies[0] : (short)-1;
            _argumentIndex = 0;

            if (_caching)
            {
                _arguments = current.Arguments = new IInterpolationArgument[
                    formattedCount - _helperStrings.Length
                ];

                _commandBuilder = null!;
                _parameters = null!;
            }
            else
            {
                _commandBuilder = current.CommandBuilder;
                _parameters = current.Parameters;

                _arguments = null!;
            }
        }

        public void AppendLiteral(string value)
        {
            if (_caching)
                return;

            _commandBuilder.Append(value);
        }

        public void AppendFormatted<T>(T value) where T : struct, IEquatable<T>
        {
            if (IsHelperString())
                return;

            if (_caching)
            {
                BaseAppendFormattedCached(value);
            }
            else
            {
                BaseAppendFormatted(value);
            }
        }

        public void AppendFormatted(object value)
        {
            if (IsHelperString())
                return;

            if (_caching)
            {
                BaseAppendFormattedCached(value);
            }
            else
            {
                BaseAppendFormatted(value);
            }
        }

        public void AppendFormatted<T>(IList<T> value) where T : struct, IEquatable<T>
        {
            if (IsHelperString())
                return;

            if (_caching)
            {
                BaseAppendFormattedCached<T>(value);
            }
            else
            {
                BaseAppendFormatted<T>(value);
            }
        }

        public void AppendFormatted(IList<object> value)
        {
            if (IsHelperString())
                return;

            if (_caching)
            {
                BaseAppendFormattedCached(value);
            }
            else
            {
                BaseAppendFormatted(value);
            }
        }

        public void AppendFormatted<T>(T value, string format) where T : struct, IEquatable<T>
        {
            _ = format;

            AppendFormatted(value);
        }

        public void AppendFormatted(object value, string format)
        {
            _ = format;

            AppendFormatted(value);
        }

        public void AppendFormatted<T>(IList<T> value, string format)
        {
            _ = format;

            AppendFormatted(value);
        }

        public void AppendFormatted(IList<object> value, string format)
        {
            _ = format;

            AppendFormatted(value);
        }

        internal void BaseAppendFormatted(object value)
        {
            var parameterName = "@p" + _absolutParameterIndex++;

            _parameters.Add(new NpgsqlParameter(parameterName, value));

            _commandBuilder.Append(parameterName);
        }

        internal void BaseAppendFormatted<T>(T value) where T : struct, IEquatable<T>
        {
            var parameterName = "@p" + _absolutParameterIndex++;

            _parameters.Add(new NpgsqlParameter(parameterName, value));

            _commandBuilder.Append(parameterName);
        }

        private void BaseAppendFormattedCached(object value)
        {
            _arguments[_argumentIndex++] = new InterpolationArgument(value);
        }

        private void BaseAppendFormattedCached<T>(T value) where T : struct, IEquatable<T>
        {
            _arguments[_argumentIndex++] = new InterpolationArgument<T>(value);
        }

        internal void BaseAppendFormatted(IList<object> values)
        {
            _commandBuilder.Append('(');

            for (var index = 0; index < values.Count; index++)
            {
                var parameterName = "@p" + _absolutParameterIndex++;

                _parameters.Add(new NpgsqlParameter(parameterName, values[index]));

                _commandBuilder.Append(parameterName).Append(',').Append(' ');
            }

            _commandBuilder.Length -= 2;
            _commandBuilder.Append(')');
        }

        internal void BaseAppendFormatted<T>(IList<T> values) where T : struct, IEquatable<T>
        {
            _commandBuilder.Append('(');

            for (var index = 0; index < values.Count; index++)
            {
                var parameterName = "@p" + _absolutParameterIndex++;

                _parameters.Add(new NpgsqlParameter<T>(parameterName, values[index]));

                _commandBuilder.Append(parameterName).Append(',').Append(' ');
            }

            _commandBuilder.Length -= 2;
            _commandBuilder.Append(')');
        }

        private void BaseAppendFormattedCached(IList<object> values)
        {
            _arguments[_argumentIndex++] = new InterpolationListArgument(values);
        }

        private void BaseAppendFormattedCached<T>(IList<T> values) where T : struct, IEquatable<T>
        {
            _arguments[_argumentIndex++] = new InterpolationListArgument<T>(values);
        }

        private bool IsHelperString()
        {
            if (_interpolationIndex++ != _nextParameterIndex)
            {
                if (!_caching)
                {
                    _commandBuilder.Append(_helperStrings![_helperStringsIndex++]);
                }

                return true;
            }

            if (++_parameterIndex < _parameterIndecies!.Length)
            {
                _nextParameterIndex = _parameterIndecies[_parameterIndex];
            }

            return false;
        }
    }
}
