using System;

namespace Venflow
{
    [Serializable]
    public class InvalidEntityRelationException : Exception
    {
        public InvalidEntityRelationException() { }
        public InvalidEntityRelationException(string message) : base(message) { }
        public InvalidEntityRelationException(string message, Exception inner) : base(message, inner) { }
        protected InvalidEntityRelationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
