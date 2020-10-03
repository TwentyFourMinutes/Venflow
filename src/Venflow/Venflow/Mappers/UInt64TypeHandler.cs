using Npgsql;
using Npgsql.BackendMessages;
using Npgsql.PostgresTypes;
using Npgsql.TypeHandling;

namespace Venflow.Mappers
{
    internal class UInt64TypeHandler : NpgsqlSimpleTypeHandler<ulong>
    {
        internal UInt64TypeHandler(PostgresType postgresType) : base(postgresType)
        {

        }

        public override ulong Read(NpgsqlReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => unchecked((ulong)(buf.ReadInt64() - long.MinValue));

        public override int ValidateAndGetLength(ulong value, NpgsqlParameter? parameter)
            => 8;

        public override void Write(ulong value, NpgsqlWriteBuffer buf, NpgsqlParameter? parameter)
            => buf.WriteInt64(unchecked((long)value + long.MinValue));
    }
}
