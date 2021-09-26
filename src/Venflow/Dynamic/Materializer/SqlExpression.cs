using Venflow.Enums;

namespace Venflow.Dynamic.Materializer
{
    internal class SqlExpression
    {
        internal string SQL { get; }
        internal Delegate Arguments { get; }
        internal Type ParameterType { get; }
        internal SqlExpressionOptions Options { get; }

        internal SqlExpression(string sql, Delegate arguments, Type parameterType, SqlExpressionOptions options)
        {
            SQL = sql;
            Arguments = arguments;
            ParameterType = parameterType;
            Options = options;
        }
    }
}
