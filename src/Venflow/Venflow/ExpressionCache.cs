using System.Linq.Expressions;

namespace Venflow.Modeling
{

    internal static class ExpressionCache
    {
        internal static readonly ParameterExpression NpgsqlDataReaderParameter = Expression.Parameter(TypeCache.NpgsqlDataReader, "dataReader");
        internal static readonly ConstantExpression TrueConstant = Expression.Constant(true);
        internal static readonly ConstantExpression FalseConstant = Expression.Constant(false);
    }
}
