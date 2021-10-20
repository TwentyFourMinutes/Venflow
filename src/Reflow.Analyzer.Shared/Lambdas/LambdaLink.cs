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
}
