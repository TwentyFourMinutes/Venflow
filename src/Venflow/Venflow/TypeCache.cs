using Npgsql;
using System;

namespace Venflow.Modeling
{
    internal static class TypeCache
    {
        internal static readonly Type NpgsqlDataReader = typeof(NpgsqlDataReader);
        internal static readonly Type Int32 = typeof(int);
        internal static readonly Type Boolean = typeof(bool);
    }
}
