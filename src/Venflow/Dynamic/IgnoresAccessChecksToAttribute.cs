using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// <strong>Do not use this attribute, if you are not absolutely sure what it does.</strong>
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public IgnoresAccessChecksToAttribute(string assemblyName)
            => AssemblyName = assemblyName;

        public string AssemblyName { get; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
