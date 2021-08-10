using System;

namespace Venflow.Dynamic.Materializer
{
    internal class SqlExpression
    {
        internal Func<object[]> Arguments { get; }
        internal (int, string)[] StaticArguments { get; }

        internal SqlExpression(Func<object[]> arguments, (int, string)[] staticArguments)
        {
            Arguments = arguments;
            StaticArguments = staticArguments;
        }
    }
}