using System.Data;
using Npgsql;

namespace Venflow
{
    internal static class NpgsqlParameterExtensions
    {
        internal static NpgsqlParameter<T> CreateParameter<T>(string parameterName, T value, DbType dbType)
        {
            var parameter = new NpgsqlParameter<T>(parameterName, dbType);

            parameter.TypedValue = value;

            return parameter;
        }
    }
}
