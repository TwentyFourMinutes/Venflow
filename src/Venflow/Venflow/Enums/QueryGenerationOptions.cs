using System;

namespace Venflow.Enums
{
    [Flags]
    internal enum QueryGenerationOptions
    {
        None = 0,
        GenerateJoins = 1,
        GenerateFullSQL = 2 | GenerateJoins
    }
}
