using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// <strong>Do not use this attribute, it is reserved for the compiler.</strong>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
    public sealed class NullableContextAttribute : Attribute
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public readonly byte Flag;

        public NullableContextAttribute(byte flag)
        {
            Flag = flag;
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
