namespace Venflow.Enums
{
    /// <summary>
    /// Represents the identity truncate option for foreign keys.
    /// </summary>
    public enum IdentityTruncateOptions : byte
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        None = 0,
        /// <summary>
        /// Automatically restart sequences owned by columns of the truncated table(s).
        /// </summary>
        Restart = 1,
        /// <summary>
        /// Do not change the values of sequences. This is the default.
        /// </summary>
        Continue = 2,
    }
}
