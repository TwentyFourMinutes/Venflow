using Npgsql;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Venflow.Dynamic.Retriever
{
    internal class ValueRetrieverFactory<TEntity> where TEntity : class, new()
    {
        private readonly Type _entityType;

        internal ValueRetrieverFactory(Type entityType)
        {
            _entityType = entityType;
        }

        internal Func<TEntity, string, NpgsqlParameter> GenerateRetriever(PropertyInfo property)
        {
            var npgsqlParameterType = typeof(NpgsqlParameter);
            var stringType = typeof(string);

            var retrieverMethod = new DynamicMethod($"Venflow.Dynamic.ValueRetrievers.{_entityType.Name}.{property.Name}ValueRetriever", npgsqlParameterType, new[] { _entityType, stringType }, TypeFactory.DynamicModule);
            var retrieverMethodIL = retrieverMethod.GetILGenerator();

            WriteDefaultRetriever(retrieverMethodIL, property);

#if NETCOREAPP5_0
            return retrieverMethod.CreateDelegate<Func<TEntity, string, NpgsqlParameter>>();
#else
            return (Func<TEntity, string, NpgsqlParameter>)retrieverMethod.CreateDelegate(typeof(Func<TEntity, string, NpgsqlParameter>));
#endif
        }

        private void WriteDefaultRetriever(ILGenerator il, PropertyInfo property)
        {
            var stringType = typeof(string);

            il.Emit(OpCodes.Ldstr, "@" + property.Name);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, property.GetGetMethod());

            var npgsqlType = property.PropertyType.IsEnum ? Enum.GetUnderlyingType(property.PropertyType) : property.PropertyType;

            il.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(npgsqlType).GetConstructor(new[] { stringType, npgsqlType }));
            il.Emit(OpCodes.Ret);
        }
    }
}
