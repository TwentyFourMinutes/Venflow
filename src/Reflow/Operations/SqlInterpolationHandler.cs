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

            _commandBuilder = current.CommandBuilder;
            _parameterIndecies = current.ParameterIndecies;
            _helperStrings = current.HelperStrings;
            _parameters = current.Parameters;
            _nextParameterIndex =
                _parameterIndecies?.Length > 0 ? _parameterIndecies[0] : (short)-1;
        }

        public void AppendLiteral(string value)
        {
            _commandBuilder.Append(value);
        }

        public void AppendFormatted<T>(T value)
        {
            BaseAppendFormatted(value);
        }

        public void AppendFormatted<T>(T value, string format)
        {
            _ = format;

            BaseAppendFormatted(value);
        }

        private void BaseAppendFormatted<T>(T value)
        {
            if (_interpolationIndex++ != _nextParameterIndex)
            {
                _commandBuilder.Append(_helperStrings![_helperStringsIndex++]);

                return;
            }

            if (++_parameterIndex < _parameterIndecies!.Length)
                _nextParameterIndex = _parameterIndecies[_parameterIndex];

            var parameterName = "@p" + _absolutParameterIndex++;

            _parameters.Add(new NpgsqlParameter<T>(parameterName, value));

            _commandBuilder.Append(parameterName);
        }
    }
}
