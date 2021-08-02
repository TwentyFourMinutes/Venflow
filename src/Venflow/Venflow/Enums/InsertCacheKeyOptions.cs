using System;

namespace Venflow.Enums
{
    [Flags]
    internal enum InsertCacheKeyOptions : byte
    {
        None = 0,
        IsFullInsert = 1 << 0,
        IsSingleInsert = 1 << 1,
        HasLogging = 1 << 2
    }
}
