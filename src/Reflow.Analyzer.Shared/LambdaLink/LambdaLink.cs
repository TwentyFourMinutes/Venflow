namespace Reflow
{
    public class LambdaLink
    {
        public string FullClassName { get; }
        public string FullLambdaName { get; }
        public string Content { get; }

        internal LambdaLink(string fullClassName, string fullLambdaName, string content)
        {
            FullClassName = fullClassName;
            FullLambdaName = fullLambdaName;
            Content = content;
        }
    }

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
