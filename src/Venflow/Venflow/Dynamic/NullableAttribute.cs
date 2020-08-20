namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// <strong>Do not use this attribute, it is reserved for the compiler.</strong>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
    public sealed class NullableAttribute : Attribute
    {
        public readonly byte[] NullableFlags;

        public NullableAttribute(byte flag)
        {
            byte[] flags = new byte[1];
            flags[0] = flag;
            NullableFlags = flags;
        }

        public NullableAttribute(byte[] flags)
        {
            NullableFlags = flags;
        }
    }
}
