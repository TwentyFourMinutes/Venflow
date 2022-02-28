namespace Reflow
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratedKey<T> : Attribute where T : struct { }

    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class GeneratedKey : Attribute
    {
        public GeneratedKey(Type type)
        {
            _ = type;
        }
    }
}
