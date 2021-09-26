using System;

namespace Venflow.Enums
{
    [Flags]
    internal enum ColumnListStringOptions : byte
    {
        None = 0,
        IncludePrimaryColumns = 1
    }
}
