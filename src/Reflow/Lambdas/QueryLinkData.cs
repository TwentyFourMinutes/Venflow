using System.Collections.Concurrent;
using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class QueryLinkData : ILambdaLinkData
    {
        internal ushort[]? ColumnIndecies { get; set; }
        internal string[]? HelperStrings { get; set; }

        internal bool Caching { get; }
        internal int MinimumSqlLength { get; }
        internal short[]? ParameterIndecies { get; }
        internal Type[] UsedEntities { get; }
        internal Delegate Parser { get; }
        internal ConcurrentDictionary<InterpolationArgumentCollection, string>? CacheKeys { get; }

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

            if (caching)
            {
                CacheKeys = new(Environment.ProcessorCount, 5);
            }
        }
    }
}
