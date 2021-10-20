using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// <strong>Do not use this attribute, it is reserved for the compiler.</strong>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
    public sealed class NullableAttribute : Attribute
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public readonly byte[] NullableFlags;

        public NullableAttribute(byte flag)
        {
            var flags = new byte[1];
            flags[0] = flag;
            NullableFlags = flags;
        }

        public NullableAttribute(byte[] flags)
        {
            NullableFlags = flags;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
