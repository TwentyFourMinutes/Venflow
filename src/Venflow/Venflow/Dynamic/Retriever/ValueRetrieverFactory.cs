using System;
using System.Reflection;
using System.Reflection.Emit;
using Npgsql;

namespace Venflow.Dynamic.Retriever
{
    internal class ValueRetrieverFactory<TEntity> where TEntity : class, new()
    {
        private readonly Type _entityType;

        internal ValueRetrieverFactory(Type entityType)
        {
            _entityType = entityType;
        }

        internal Func<TEntity, string, NpgsqlParameter> GenerateRetriever(PropertyInfo property, bool isPostgreEnum)
        {
            var npgsqlParameterType = typeof(NpgsqlParameter);
            var stringType = typeof(string);

            var retrieverMethod = new DynamicMethod($"Venflow.Dynamic.ValueRetrievers.{_entityType.Name}.{property.Name}ValueRetriever", npgsqlParameterType, new[] { _entityType, stringType }, TypeFactory.DynamicModule);
            var retrieverMethodIL = retrieverMethod.GetILGenerator();

            var underlyingType = Nullable.GetUnderlyingType(property.PropertyType);

            if (underlyingType is { } &&
                underlyingType.IsEnum &&
                !isPostgreEnum)
            {
                WriteNullableRetriever(retrieverMethodIL, property, Enum.GetUnderlyingType(underlyingType));
            }
            else if (underlyingType is { })
            {
                WriteNullableRetriever(retrieverMethodIL, property, underlyingType);
            }
            else
            {
                WriteDefaultRetriever(retrieverMethodIL, property, property.PropertyType.IsEnum && !isPostgreEnum ? Enum.GetUnderlyingType(property.PropertyType) : property.PropertyType);
            }

#if NETCOREAPP5_0
            return retrieverMethod.CreateDelegate<Func<TEntity, string, NpgsqlParameter>>();
#else
            return (Func<TEntity, string, NpgsqlParameter>)retrieverMethod.CreateDelegate(typeof(Func<TEntity, string, NpgsqlParameter>));
#endif
        }

        private void WriteDefaultRetriever(ILGenerator il, PropertyInfo property, Type underylingType)
        {
            var stringType = typeof(string);

            il.Emit(OpCodes.Ldstr, "@p" + property.Name);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, property.GetGetMethod());

            if (underylingType == typeof(ulong))
            {
                underylingType = typeof(long);

                il.Emit(OpCodes.Ldc_I8, long.MinValue);
                il.Emit(OpCodes.Add);
            }

            il.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(underylingType).GetConstructor(new[] { stringType, underylingType }));
            il.Emit(OpCodes.Ret);
        }

        private void WriteNullableRetriever(ILGenerator il, PropertyInfo property, Type underlyingType)
        {
            var stringType = typeof(string);
            var dbNullType = typeof(DBNull);

            var stringConcatMethod = stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null);

            var propertyLocal = il.DeclareLocal(property.PropertyType);

            var defaultRetrieverLabel = il.DefineLabel();

            // Check if property has value
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, property.GetGetMethod());
            il.Emit(OpCodes.Stloc_S, propertyLocal);
            il.Emit(OpCodes.Ldloca_S, propertyLocal);
            il.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("HasValue").GetGetMethod());
            il.Emit(OpCodes.Brtrue_S, defaultRetrieverLabel);

            // Nullable retriever
            il.Emit(OpCodes.Ldstr, "@p" + property.Name);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, stringConcatMethod);
            il.Emit(OpCodes.Ldsfld, dbNullType.GetField("Value"));
            il.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(dbNullType).GetConstructor(new[] { stringType, dbNullType }));
            il.Emit(OpCodes.Ret);

            // Default retriever
            il.MarkLabel(defaultRetrieverLabel);

            il.Emit(OpCodes.Ldstr, "@p" + property.Name);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, stringConcatMethod);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, property.GetGetMethod());
            il.Emit(OpCodes.Stloc_S, propertyLocal);
            il.Emit(OpCodes.Ldloca_S, propertyLocal);
            il.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("Value").GetGetMethod());

            if (underlyingType == typeof(ulong))
            {
                underlyingType = typeof(long);

                il.Emit(OpCodes.Ldc_I8, long.MinValue);
                il.Emit(OpCodes.Add);
            }

            il.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(underlyingType).GetConstructor(new[] { stringType, underlyingType }));
            il.Emit(OpCodes.Ret);
        }
    }
}
