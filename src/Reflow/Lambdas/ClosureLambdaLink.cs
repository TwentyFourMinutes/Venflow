namespace Reflow.Lambdas
{
    public class ClosureLambdaLink : LambdaLink
    {
        internal int MemberIndex { get; }

        public ClosureLambdaLink(
            Type classType,
            int memberIndex,
            string fullLambdaName,
            LambdaData data
        ) : base(classType, fullLambdaName, data)
        {
            MemberIndex = memberIndex;
        }
    }
}
