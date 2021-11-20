using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LambdaLink
    {
        internal Type ClassType { get; }
        internal string IdentifierName { get; }
        internal uint LambdaIndex { get; }
        internal bool HasClosure { get; }
        internal ILambdaLinkData Data { get; }

        public LambdaLink(
            Type classType,
            string identifierName,
            uint lambdaIndex,
            bool hasClosure,
            ILambdaLinkData data
        )
        {
            ClassType = classType;
            IdentifierName = identifierName;
            LambdaIndex = lambdaIndex;
            HasClosure = hasClosure;
            Data = data;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILambdaLinkData
    {
    }
}
