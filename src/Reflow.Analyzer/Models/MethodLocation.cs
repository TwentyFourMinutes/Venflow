namespace Reflow.Analyzer.Models
{
    internal class MethodLocation
    {
        internal string FullTypeName { get; }
        internal string MethodName { get; }

        internal MethodLocation(string fullTypeName, string methodName)
        {
            FullTypeName = fullTypeName;
            MethodName = methodName;
        }
    }
}
