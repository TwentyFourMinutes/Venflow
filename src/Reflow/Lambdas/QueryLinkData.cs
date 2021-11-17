using System.ComponentModel;

namespace Reflow.Lambdas
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class QueryLinkData : ILambdaLinkData
    {
        internal short[]? ColumnIndecies { get; set; }

        internal int MinimumSqlLength { get; }
        internal short[] ParameterIndecies { get; }
        internal Type[]? UsedEntities { get; }
        internal MethodLocation Location { get; }

        public QueryLinkData(
            int minimumSqlLength,
            short[] parameterIndecies,
            Type[] usedEntities,
            MethodLocation location
        )
        {
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            UsedEntities = usedEntities;
            Location = location;
        }
    }
}
