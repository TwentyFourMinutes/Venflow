using Newtonsoft.Json;

namespace Reflow.Analyzer.Models.Definitions
{
    internal class LambdaLinkDefinition
    {
        internal ILambdaLinkData? Data { get; set; }

        [JsonProperty]
        internal string ClassName { get; }

        [JsonProperty]
        internal string IdentifierName { get; }

        [JsonProperty]
        internal uint LambdaIndex { get; set; }

        [JsonProperty]
        internal bool HasClosure { get; }

        [JsonConstructor]
        internal LambdaLinkDefinition(
            string className,
            string identifierName,
            uint lambdaIndex,
            bool hasClosure
        )
        {
            ClassName = className;
            IdentifierName = identifierName;
            LambdaIndex = lambdaIndex;
            HasClosure = hasClosure;
        }
    }

    internal interface ILambdaLinkData { }
}
