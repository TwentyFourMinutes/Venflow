using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Npgsql;

namespace Venflow
{
    /// <summary>
    /// Allows for a safe string interpolated SQL concatenation.
    /// </summary>
    public class FormattableSqlStringBuilder
    {
        internal List<NpgsqlParameter> Parameters { get; }

        private StringBuilder? _sql;
        private int _parameterIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="FormattableSqlStringBuilder"/> class.
        /// </summary>
        public FormattableSqlStringBuilder()
        {
            _sql = default;
            Parameters = new List<NpgsqlParameter>();
        }

        /// <summary>
        /// Appends a copy of the specified SQL followed by the default line terminator to the end of the current <see cref="FormattableSqlStringBuilder"/> object.
        /// </summary>
        /// <param name="sql">The SQL to append.</param>
        /// <remarks>
        /// <b>This method IS NOT string interpolation safe</b>, if you want to use string interpolated SQL use the <see cref="AppendInterpolatedLine(FormattableString)"/> method.
        /// </remarks>
        public void AppendLine(string sql)
        {
            if (_sql is null)
            {
                _sql = new StringBuilder(sql);

                _sql.AppendLine();
            }
            else
            {
                _sql.AppendLine(sql);
            }
        }

        /// <summary>
        /// Appends a copy of the specified SQL to this instance.
        /// </summary>
        /// <param name="sql">The SQL to append.</param>
        /// <remarks>
        /// <b>This method IS NOT string interpolation safe</b>, if you want to use string interpolated SQL use the <see cref="AppendInterpolated(FormattableString)"/> method.
        /// </remarks>
        public void Append(string sql)
        {
            if (_sql is null)
                _sql = new StringBuilder(sql);
            else
                _sql.Append(sql);
        }

        /// <summary>
        /// Appends a copy of the specified SQL followed by the default line terminator to the end of the current <see cref="FormattableSqlStringBuilder"/> object.
        /// </summary>
        /// <param name="sql">The SQL to append.</param>
        /// <remarks>
        /// This method is string interpolation safe.
        /// </remarks>
        public void AppendInterpolatedLine(FormattableString sql)
        {
            AppendInterpolated(sql);

            _sql.AppendLine();
        }

        /// <summary>
        /// Appends a copy of the specified SQL to this instance.
        /// </summary>
        /// <param name="sql">The SQL to append.</param>
        /// <remarks>
        /// This method is string interpolation safe.
        /// </remarks>
        public void AppendInterpolated(FormattableString sql)
        {
            var argumentsSpan = sql.GetArguments().AsSpan();

            var sqlLength = sql.Format.Length;

            var sqlSpan = sql.Format.AsSpan();

            if (_sql is null)
                _sql = new StringBuilder(sql.Format.Length);

            var argumentIndex = 0;

            for (int spanIndex = 0; spanIndex < sqlLength; spanIndex++)
            {
                var spanChar = sqlSpan[spanIndex];

                if (spanChar == '{' &&
                    spanIndex + 2 < sqlLength)
                {
                    for (spanIndex++; spanIndex < sqlLength; spanIndex++)
                    {
                        spanChar = sqlSpan[spanIndex];

                        if (spanChar == '}')
                            break;

                        if (spanChar is < '0' or > '9')
                            throw new InvalidOperationException();
                    }

                    var argument = argumentsSpan[argumentIndex++];

                    HandleParameter(argument);
                }
                else
                {
                    _sql.Append(spanChar);
                }
            }
        }

        /// <summary>
        /// Appends a parameter of to this instance.
        /// </summary>
        /// <typeparam name="T">The type of the parameter value.</typeparam>
        /// <param name="value">The parameter value.</param>
        public void AppendParameter<T>(T value)
        {
            if (_sql is null)
                _sql = new StringBuilder();

            HandleParameter(value);
        }

#if NET5_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private void HandleParameter<T>(T value)
        {
            if (value is IList list)
            {
                if (list.Count > 0)
                {
                    var listType = default(Type);

                    for (int listIndex = 0; listIndex < list.Count; listIndex++)
                    {
                        var listItem = list[listIndex];

                        if (listType is null &&
                            listItem is not null)
                        {
                            listType = listItem.GetType();

                            if (listType == typeof(object))
                                throw new InvalidOperationException("The SQL string interpolation doesn't support object lists.");
                        }

                        var parameterName = "@p" + _parameterIndex++.ToString();

                        _sql.Append(parameterName)
                            .Append(", ");

                        Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, listType!, listItem));
                    }

                    _sql.Length -= 2;
                }

                _parameterIndex--;
            }
            else
            {
                var parameterName = "@p" + _parameterIndex++.ToString();

                _sql.Append(parameterName);

                Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, value));
            }
        }

        internal string Build()
        {
            if (_sql is null || _sql.Length == 0)
                throw new InvalidOperationException($"You have to populate the {nameof(FormattableSqlStringBuilder)} instance before you can inject it into a query method.");

            return _sql.ToString();
        }
    }
}
