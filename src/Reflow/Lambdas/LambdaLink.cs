using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LambdaLink
    {
        internal Type DatabaseType { get; }
        internal Type ClassType { get; }
        internal string IdentifierName { get; }
        internal uint LambdaIndex { get; }
        internal bool HasClosure { get; }
        internal ILambdaLinkData Data { get; }

        public LambdaLink(
            Type databaseType,
            Type classType,
            string identifierName,
            uint lambdaIndex,
            bool hasClosure,
            ILambdaLinkData data
        )
        {
            DatabaseType = databaseType;
            ClassType = classType;
            IdentifierName = identifierName;
            LambdaIndex = lambdaIndex;
            HasClosure = hasClosure;
            Data = data;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILambdaLinkData { }
}
