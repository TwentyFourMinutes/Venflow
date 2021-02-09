namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// <strong>Do not use this attribute, if you are not absolutely sure what it does.</strong>
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
            => AssemblyName = assemblyName;

        public string AssemblyName { get; }
    }
}