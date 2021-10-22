namespace Reflow
{
    public class ClosureLambdaLink : LambdaLink
    {
        internal int MemberIndex { get; }

        public ClosureLambdaLink(
            string fullClassName,
            int memberIndex,
            string fullLambdaName,
            LambdaData data
        ) : base(fullClassName, fullLambdaName, data)
        {
            MemberIndex = memberIndex;
        }
    }
}
