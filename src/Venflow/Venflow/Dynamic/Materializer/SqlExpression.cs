using System;
using Venflow.Enums;

namespace Venflow.Dynamic.Materializer
{
    internal class SqlExpression
    {
        internal Delegate Arguments { get; }
        internal Type ParameterType { get; }
        internal SqlExpressionOptions Options { get; }
        internal (int, string)[] StaticArguments { get; }

        internal SqlExpression(Delegate arguments, Type parameterType, SqlExpressionOptions options, (int, string)[] staticArguments)
        {
            Arguments = arguments;
            ParameterType = parameterType;
            Options = options;
            StaticArguments = staticArguments;
        }
    }
}