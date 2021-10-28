namespace Reflow.Lambdas
{
    public class LambdaData
    {
        internal short[]? ColumnIndecies { get; set; }
        internal Type[]? UsedEntities { get; set; }

        internal int MinimumSqlLength { get; }
        internal short[] ParameterIndecies { get; }

        public LambdaData(int minimumSqlLength, short[] parameterIndecies, Type[] usedEntities)
        {
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            UsedEntities = usedEntities;
        }
    }
}
