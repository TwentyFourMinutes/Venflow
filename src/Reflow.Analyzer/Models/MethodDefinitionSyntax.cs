namespace Reflow.Analyzer.Models
{
    internal class MethodDefinitionSyntax
    {
        public string FullTypeName { get; }
        public string MethodName { get; }

        internal MethodDefinitionSyntax(string fullTypeName, string methodName)
        {
            FullTypeName = fullTypeName;
            MethodName = methodName;
        }
    }
}
