using System;

namespace Venflow.Enums
{
    [Flags]
    public enum CommandType : short
    {
        QuerySingle = 1 << 0,
        QueryBatch = 1 << 1,
        Query = QuerySingle | QueryBatch,

        UpdateSingle = 1 << 2,
        UpdateBatch = 1 << 3,
        Update = UpdateSingle | UpdateBatch,

        InsertSingle = 1 << 4,
        InsertBatch = 1 << 5,
        Insert = InsertSingle | InsertBatch,

        DeleteSingle = 1 << 6,
        DeleteBatch = 1 << 7,
        Delete = DeleteSingle | DeleteBatch,
    }
}
