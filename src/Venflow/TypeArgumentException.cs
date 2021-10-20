namespace Venflow
{
    /// <summary>
    /// Represents an error which occur when an invalid type argument gets passed to a generic method.
    /// </summary>
    [Serializable]
    public class TypeArgumentException : Exception
    {
        /// <inheritdoc/>
        public TypeArgumentException() : base() { }

        /// <inheritdoc/>
        public TypeArgumentException(string message) : base(message) { }

        /// <inheritdoc/>
        public TypeArgumentException(string message, string type) : base(message + " Type: " + type) { }

        /// <inheritdoc/>
        public TypeArgumentException(string message, Exception inner) : base(message, inner) { }
    }
}
