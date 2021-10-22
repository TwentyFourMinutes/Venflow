using System.Runtime.CompilerServices;
using System.Text;
using Npgsql;

namespace Reflow.Commands
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
        }

        private int _interpolationIndex;
        private short _parameterIndex;
        private short _absolutParameterIndex;
        private short _nextParameterIndex;

        private readonly StringBuilder _commandBuilder;
        private readonly short[] _parameterIndecies;
        private readonly NpgsqlParameterCollection _parameters;

        public SqlInterpolationHandler(int literalLength, int formattedCount)
        {
            _ = literalLength;
            _ = formattedCount;

            _interpolationIndex = 0;
            _parameterIndex = 0;
            _absolutParameterIndex = 0;

            var current = AmbientData.Current;

            if (current is null)
                throw new InvalidOperationException($"Invalid usage of the '{nameof(SqlInterpolationHandler)}' struct, it should never be called explictly by user code.");

            _commandBuilder = current.CommandBuilder;
            _parameterIndecies = current.ParameterIndecies;
            _parameters = current.Parameters;
            _nextParameterIndex = _parameterIndecies.Length > 0 ? _parameterIndecies[0] : (short)-1;
        }

        public void AppendLiteral(string value)
        {
            _commandBuilder.Append(value);
        }

        public void AppendFormatted<T>(T value)
        {
            BaseAppendFormatted(value);
        }

        private void BaseAppendFormatted<T>(T value)
        {
            if (_interpolationIndex++ != _nextParameterIndex)
            {
                return;
            }

            if (++_parameterIndex < _parameterIndecies.Length)
                _nextParameterIndex = _parameterIndecies[_parameterIndex];

            var parameterName = "@p" + _absolutParameterIndex++;

            _parameters.Add(new NpgsqlParameter<T>(parameterName, value));

            _commandBuilder.Append(parameterName);
        }
    }
}
