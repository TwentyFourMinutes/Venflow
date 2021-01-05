namespace Venflow.Enums
{
    /// <summary>
    /// Specifies the logging behavior for Venflow commands.
    /// </summary>
    public enum LoggingBehavior : byte
    {
        /// <summary>
        /// Logs all commands.
        /// </summary>
        Always,
        /// <summary>
        /// Never logs commands.
        /// </summary>
        Never
    }
}
