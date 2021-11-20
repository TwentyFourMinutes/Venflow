using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class QueryLinkData : ILambdaLinkData
    {
        internal ushort[]? ColumnIndecies { get; set; }

        internal int MinimumSqlLength { get; }
        internal short[] ParameterIndecies { get; }
        internal Type[]? UsedEntities { get; }
        internal Delegate Parser { get; }

        public QueryLinkData(
            int minimumSqlLength,
            short[] parameterIndecies,
            Type[] usedEntities,
            Delegate parser
        )
        {
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            UsedEntities = usedEntities;
            Parser = parser;
        }
    }
}
