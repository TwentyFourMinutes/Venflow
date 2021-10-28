namespace Reflow
{
    public class LambdaData
    {
        internal short[]? ColumnIndecies { get; set; }
        internal string[]? UsedEntities { get; set; }

        internal int MinimumSqlLength { get; }
        internal short[] ParameterIndecies { get; }

        public LambdaData(int minimumSqlLength, short[] parameterIndecies, string[] usedEntities)
        {
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
            UsedEntities = usedEntities;
        }
    }
}
