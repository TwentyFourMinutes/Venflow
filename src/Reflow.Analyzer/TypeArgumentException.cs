namespace Reflow.Analyzer
{
    [Serializable]
    public class TypeArgumentException : Exception
    {
        public TypeArgumentException() { }

        public TypeArgumentException(string message) : base(message) { }

        public TypeArgumentException(string message, Exception inner) : base(message, inner) { }

        protected TypeArgumentException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context
        ) : base(info, context) { }
    }
}
