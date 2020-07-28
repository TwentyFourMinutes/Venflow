using System.Drawing;

namespace Venflow.Enums
{
    /// <summary>
    /// Represents the truncate option for foreign keys.
    /// </summary>
    public enum ForeignTruncateOptions : byte
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        None = 0,
        /// <summary>
        /// Automatically truncate all tables that have foreign-key references to any of the named tables, or to any tables added to the group due to CASCADE.
        /// </summary>
        Cascade = 1,
        /// <summary>
        /// Refuse to truncate if any of the tables have foreign-key references from tables that are not listed in the command. This is the default.
        /// </summary>
        Restrict = 2
    }
}
