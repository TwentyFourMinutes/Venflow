using System.Data;
using Npgsql;
using Npgsql.PostgresTypes;
using Npgsql.TypeHandling;
using Npgsql.TypeMapping;

namespace Venflow.Mappers
{
    internal class UInt64HandlerFactory : NpgsqlTypeHandlerFactory<ulong>
    {
        internal static void ApplyMapping()
        {
            NpgsqlConnection.GlobalTypeMapper.AddMapping(new NpgsqlTypeMappingBuilder()
            {
                DbTypes = new[] { DbType.UInt64 },
                PgTypeName = "bigint",
                NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Bigint,
                ClrTypes = new[] { typeof(ulong) },
                TypeHandlerFactory = new UInt64HandlerFactory()

            }.Build());
        }

        public override NpgsqlTypeHandler<ulong> Create(PostgresType pgType, NpgsqlConnection conn)
            => new UInt64TypeHandler(pgType);
    }
}
