using System;

namespace Venflow.Enums
{
    /// <summary>
    /// Represents the truncate options for a insert operation.
    /// </summary>
    [Flags]
    public enum InsertOptions
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        None = 0,
        /// <summary>
        /// Populates all navigation properties.
        /// </summary>
        PopulateRelations = 1,
        /// <summary>
        /// Get the database generated columns after inserting and populates all navigation properties.
        /// </summary>
        SetIdentityColumns = 2 | PopulateRelations
    }
}
