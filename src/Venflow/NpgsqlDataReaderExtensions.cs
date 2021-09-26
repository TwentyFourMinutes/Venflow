using Npgsql;

namespace Venflow
{
    internal static class NpgsqlDataReaderExtensions
    {
        internal static T GetValueOrDefault<T>(this NpgsqlDataReader reader, int ordinal)
            => reader.IsDBNull(ordinal) ? default! : reader.GetFieldValue<T>(ordinal);
    }
}
