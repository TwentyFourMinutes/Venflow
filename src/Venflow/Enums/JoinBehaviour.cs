namespace Venflow.Enums
{
    /// <summary>
    /// Specifies the behavior for a specific join between two tables.
    /// </summary>
    public enum JoinBehaviour : byte
    {
        /// <summary>
        /// Returns records that have matching values in both tables
        /// </summary>
        InnerJoin,
        /// <summary>
        /// Returns all records from the left table, and the matched records from the right table
        /// </summary>
        LeftJoin,
        /// <summary>
        /// Returns all records from the right table, and the matched records from the left table
        /// </summary>
        RightJoin,
        /// <summary>
        /// Returns all records when there is a match in either left or right table
        /// </summary>
        FullJoin
    }
}
