using System;

namespace Venflow.Enums
{
    [Flags]
    internal enum ColumnListStringOptions : byte
    {
        None = 0,
        IncludePrimaryColumns = 1,
        ExplicitNames = 2,
        PrefixedPrimaryKeys = 3
    }
}