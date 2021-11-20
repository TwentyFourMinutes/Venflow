namespace Reflow.Analyzer.Models.Definitions
{
    internal class LambdaLinkDefinition
    {
        internal ILambdaLinkData? Data { get; set; }

        internal string ClassName { get; }
        internal string IdentifierName { get; }
        internal uint LambdaIndex { get; set; }
        internal bool HasClosure { get; }

        internal LambdaLinkDefinition(
            string className,
            string identifierName,
            uint lambdaIndex,
            bool hasClosure
        )
        {
            ClassName = className;
            IdentifierName = identifierName;
            LambdaIndex = lambdaIndex;
            HasClosure = hasClosure;
        }
    }

    internal interface ILambdaLinkData
    {
    }
}
