using System;

namespace Venflow
{
    /// <summary>
    /// Represents an error which occur when an invalid type argument gets passed to a generic method.
    /// </summary>
    [Serializable]
    public class TypeArgumentException : Exception
    {
        /// <inheritdoc/>
        public TypeArgumentException(string type) : base($"The provided generic type argument '{type}' for the method was not valid.") { }

        /// <inheritdoc/>
        public TypeArgumentException(string message, string type) : base(message + " Type: " + type) { }

        /// <inheritdoc/>
        public TypeArgumentException(string message, Exception inner) : base(message, inner) { }
    }
}
