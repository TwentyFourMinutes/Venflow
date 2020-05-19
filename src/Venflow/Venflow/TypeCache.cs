using Npgsql;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Venflow.Modeling
{
    internal static class TypeCache
    {
        internal static readonly Type NpgsqlDataReader = typeof(NpgsqlDataReader);
        internal static readonly Type Int32 = typeof(int);
        internal static readonly Type Boolean = typeof(bool);
        internal static readonly Type NotMappedAttribute = typeof(NotMappedAttribute);
        internal static readonly Type NpgsqlParameter = typeof(NpgsqlParameter);
        internal static readonly Type GenericNpgsqlParameter = typeof(NpgsqlParameter<>);
        internal static readonly Type Object = typeof(object);
        internal static readonly Type String = typeof(string);
        internal static readonly Type StringBuilder = typeof(StringBuilder);
        internal static readonly Type NpgsqlParameterCollection = typeof(NpgsqlParameterCollection);
    }
}
