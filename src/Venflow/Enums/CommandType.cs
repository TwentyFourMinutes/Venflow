using System;

namespace Venflow.Enums
{
    /// <summary>
    /// Specifies the type of command that produced a log.
    /// </summary>
    [Flags]
    public enum CommandType : short
    {
        /// <summary>
        /// A query command returning a single entity.
        /// </summary>
        QuerySingle = 1 << 0,
        /// <summary>
        /// A query command returning a batch of entities.
        /// </summary>
        QueryBatch = 1 << 1,
        /// <summary>
        /// All query commands.
        /// </summary>
        Query = QuerySingle | QueryBatch,

        /// <summary>
        /// A update command updating a single entity.
        /// </summary>
        UpdateSingle = 1 << 2,
        /// <summary>
        /// A update command updating a batch of entities.
        /// </summary>
        UpdateBatch = 1 << 3,
        /// <summary>
        /// All update commands.
        /// </summary>
        Update = UpdateSingle | UpdateBatch,

        /// <summary>
        /// An insert command inserting a single entity.
        /// </summary>
        InsertSingle = 1 << 4,
        /// <summary>
        /// An insert command inserting a batch of entities.
        /// </summary>
        InsertBatch = 1 << 5,
        /// <summary>
        /// All insert commands.
        /// </summary>
        Insert = InsertSingle | InsertBatch,

        /// <summary>
        /// A delete command deleting a single entity.
        /// </summary>
        DeleteSingle = 1 << 6,
        /// <summary>
        /// A delete command deleting a batch of entities.
        /// </summary>
        DeleteBatch = 1 << 7,
        /// <summary>
        /// All delete commands.
        /// </summary>
        Delete = DeleteSingle | DeleteBatch,
    }
}
