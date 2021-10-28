namespace Reflow.Lambdas
{
    public class LambdaLink
    {
        internal Type ClassType { get; }
        internal string FullLambdaName { get; }
        internal LambdaData Data { get; }

        public LambdaLink(Type classType, string fullLambdaName, LambdaData data)
        {
            ClassType = classType;
            FullLambdaName = fullLambdaName;
            Data = data;
        }
    }
}
