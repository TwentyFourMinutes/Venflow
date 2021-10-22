namespace Reflow
{
    public class LambdaData
    {
        internal short[]? ColumnIndecies { get; set; }

        internal int MinimumSqlLength { get; }
        internal short[] ParameterIndecies { get; }

        public LambdaData(int minimumSqlLength, short[] parameterIndecies)
        {
            MinimumSqlLength = minimumSqlLength;
            ParameterIndecies = parameterIndecies;
        }
    }
}
