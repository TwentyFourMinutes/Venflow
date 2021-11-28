using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class QueryLinkData : ILambdaLinkData
    {
        internal ushort[]? ColumnIndecies { get; set; }
        internal string[]? HelperStrings { get; set; }

        internal int MinimumSqlLength { get; }
        internal short[]? ParameterIndecies { get; }
        internal Type[] UsedEntities { get; }
        internal Delegate Parser { get; }

        public QueryLinkData(
            int minimumSqlLength,
            short[]? parameterIndecies,
            string[]? helperStrings,
            Type[] usedEntities,
            Delegate parser
        )
        {
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            HelperStrings = helperStrings;
            UsedEntities = usedEntities;
            Parser = parser;
        }
    }
}
