namespace Reflow.Analyzer.Models.Definitions
{
    internal class LambdaLinkDefinition
    {
        internal ILambdaLinkData? Data { get; set; }

        internal string ClassName { get; }
        internal string IdentifierName { get; }
        internal int MemberIndex { get; }
        internal int LambdaIndex { get; }
        internal bool HasClosure { get; }

        internal LambdaLinkDefinition(
            string className,
            string identifierName,
            int memberIndex,
            int lambdaIndex,
            bool hasClosure
        )
        {
            ClassName = className;
            IdentifierName = identifierName;
            MemberIndex = memberIndex;
            LambdaIndex = lambdaIndex;
            HasClosure = hasClosure;
        }
    }

    internal interface ILambdaLinkData
    {
    }
}
