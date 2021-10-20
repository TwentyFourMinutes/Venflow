namespace Reflow
{
    public class ClosureLambdaLink : LambdaLink
    {
        public int MemberIndex { get; }

        internal ClosureLambdaLink(
            string fullClassName,
            int memberIndex,
            string fullLambdaName,
            string content
        ) : base(fullClassName, fullLambdaName, content)
        {
            MemberIndex = memberIndex;
        }
    }
}
