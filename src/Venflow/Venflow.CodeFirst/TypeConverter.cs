using System;
using NpgsqlTypes;

namespace Venflow.CodeFirst
{
    internal static class TypeConverter
    {
        internal static NpgsqlDbType GetPostgresType(Type type)
            => Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => NpgsqlDbType.Boolean,
                TypeCode.Int16 => NpgsqlDbType.Smallint,
                TypeCode.Int32 => NpgsqlDbType.Integer,
                TypeCode.Int64 => NpgsqlDbType.Bigint,
                TypeCode.UInt64 => NpgsqlDbType.Bigint,
                TypeCode.Single => NpgsqlDbType.Real,
                TypeCode.Double => NpgsqlDbType.Double,
                TypeCode.Decimal => NpgsqlDbType.Numeric,
                TypeCode.String => NpgsqlDbType.Text,
                TypeCode.String => NpgsqlDbType.Text,
            };
    }
}
