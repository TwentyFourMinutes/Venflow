namespace Reflow
{
    public class LambdaLink
    {
        internal string FullClassName { get; }
        internal string FullLambdaName { get; }
        internal LambdaData Data { get; }

        public LambdaLink(string fullClassName, string fullLambdaName, LambdaData data)
        {
            FullClassName = fullClassName;
            FullLambdaName = fullLambdaName;
            Data = data;
        }
    }
}
