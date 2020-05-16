using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Venflow.Modeling
{

    internal static class ExpressionCache
    {
        internal static readonly ParameterExpression NpgsqlDataReaderParameter = Expression.Parameter(TypeCache.NpgsqlDataReader, "dataReader");
        internal static readonly ConstantExpression TrueConstant = Expression.Constant(true);
        internal static readonly ConstantExpression FalseConstant = Expression.Constant(false);
    }
}
