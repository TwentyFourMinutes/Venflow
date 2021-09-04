using System;

namespace Venflow.Enums
{
    [Flags]
    internal enum ColumnOptions : byte
    {
        None = 0,
        NullableReferenceType = 1 << 0,
        ReadOnly = 1 << 1,
        PrimaryKey = 1 << 2,
        PostgreEnum = 1 << 3,
        Generated = 1 << 4
    }
}
