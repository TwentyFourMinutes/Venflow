namespace Reflow.Analyzer.Models
{
    internal class MethodLocation
    {
        public string FullTypeName { get; }
        public string MethodName { get; }

        internal MethodLocation(string fullTypeName, string methodName)
        {
            FullTypeName = fullTypeName;
            MethodName = methodName;
        }
    }
}
