using Npgsql;

namespace Venflow
{
    public static class NpgsqlDataReaderExtensions
    {
        public static T GetValueOrDefault<T>(this NpgsqlDataReader reader, int ordinal)
            => reader.IsDBNull(ordinal) ? default! : reader.GetFieldValue<T>(ordinal);
    }
}
