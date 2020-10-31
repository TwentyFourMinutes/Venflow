using System;
using System.Linq;
using System.Linq.Expressions;
using Npgsql;
using NpgsqlTypes;

namespace Venflow.CodeFirst
{
    internal static class NpgsqlTypeMapper
    {
        private static readonly object _typeMapperInstance;
        private static readonly Func<object, Type, NpgsqlDbType> _typeMapper;
        static NpgsqlTypeMapper()
        {
            var globalTypeMapperType = typeof(NpgsqlCommand).Assembly.GetTypes().First(x => x.Name == "GlobalTypeMapper");
            _typeMapperInstance = globalTypeMapperType.GetProperty("Instance").GetValue(null);

            var mapperMethod = globalTypeMapperType.GetMethod("ToNpgsqlDbType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, new[] { typeof(Type) }, null);

            var typeMapperParameter = Expression.Parameter(typeof(object));
            var typeParameter = Expression.Parameter(typeof(Type));

            _typeMapper = Expression.Lambda<Func<object, Type, NpgsqlDbType>>(Expression.Call(Expression.Convert(typeMapperParameter, globalTypeMapperType), mapperMethod, typeParameter), typeMapperParameter, typeParameter).Compile();
        }
        internal static NpgsqlDbType GetDbType(Type type)
        {
            return _typeMapper.Invoke(_typeMapperInstance, type);
        }
    }
}
