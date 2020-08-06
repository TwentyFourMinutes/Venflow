using System;

namespace Venflow
{
    /// <summary>
    /// Represents errors that occur during the relation entity generation.
    /// </summary>
    [Serializable]
    public class InvalidEntityRelationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEntityRelationException"/> class.
        /// </summary>
        public InvalidEntityRelationException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEntityRelationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public InvalidEntityRelationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEntityRelationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public InvalidEntityRelationException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidEntityRelationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">info is null.</exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">The class name is null or System.Exception.HResult is zero (0).</exception>
        protected InvalidEntityRelationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
