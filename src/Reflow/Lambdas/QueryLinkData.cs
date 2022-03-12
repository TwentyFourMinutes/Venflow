using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class QueryLinkData : ILambdaLinkData
    {
        internal ushort[]? ColumnIndecies { get; set; }
        internal string[]? HelperStrings { get; set; }

        internal QueryCacheKey? CacheKey { get; set; }

        internal bool Caching { get; }
        internal int MinimumSqlLength { get; }
        internal short[]? ParameterIndecies { get; }
        internal Type[] UsedEntities { get; }
        internal Delegate Parser { get; }

        public QueryLinkData(
            bool caching,
            int minimumSqlLength,
            short[]? parameterIndecies,
            string[]? helperStrings,
            Type[] usedEntities,
            Delegate parser
        )
        {
            Caching = caching;
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            HelperStrings = helperStrings;
            UsedEntities = usedEntities;
            Parser = parser;
        }
    }
}
