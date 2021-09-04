using System;
using System.Reflection;
using System.Reflection.Emit;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling.Definitions;

namespace Venflow.Dynamic.Retriever
{
    internal class ValueRetrieverFactory<TEntity> where TEntity : class, new()
    {
        private readonly Type _entityType;

        internal ValueRetrieverFactory(Type entityType)
        {
            _entityType = entityType;
        }

        internal Func<TEntity, string, NpgsqlParameter> GenerateRetriever(ColumnDefinition column)
        {
            var npgsqlParameterType = typeof(NpgsqlParameter);
            var stringType = typeof(string);

            var retrieverMethod = TypeFactory.GetDynamicMethod($"Venflow.Dynamic.ValueRetrievers.{_entityType.Name}.{column.Property.Name}ValueRetriever", npgsqlParameterType, new[] { _entityType, stringType });
            var retrieverMethodIL = retrieverMethod.GetILGenerator();

            var underlyingType = Nullable.GetUnderlyingType(column.Property.PropertyType);

            var isPostgreEnum = column.Options.HasFlag(ColumnOptions.PostgreEnum);

            if (underlyingType is not null &&
                underlyingType.IsEnum &&
                !isPostgreEnum)
            {
                WriteNullableRetriever(retrieverMethodIL, column, Enum.GetUnderlyingType(underlyingType));
            }
            else if (underlyingType is not null)
            {
                WriteNullableRetriever(retrieverMethodIL, column, underlyingType);
            }
            else
            {
                WriteDefaultRetriever(retrieverMethodIL, column, column.Property.PropertyType.IsEnum && !isPostgreEnum ? Enum.GetUnderlyingType(column.Property.PropertyType) : column.Property.PropertyType);
            }

#if NET5_0_OR_GREATER
            return retrieverMethod.CreateDelegate<Func<TEntity, string, NpgsqlParameter>>();
#else
            return (Func<TEntity, string, NpgsqlParameter>)retrieverMethod.CreateDelegate(typeof(Func<TEntity, string, NpgsqlParameter>));
#endif
        }

        private void WriteDefaultRetriever(ILGenerator il, ColumnDefinition column, Type underlyingType)
        {
            var stringType = typeof(string);

            il.Emit(OpCodes.Ldstr, "@p" + column.Property.Name);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, column.Property.GetGetMethod());

            if (typeof(IKey).IsAssignableFrom(underlyingType))
            {
                var underlyingStronglyTypedIdType = underlyingType.GetInterface(typeof(IKey<,>).Name).GetGenericArguments()[1];

                var keyLocal = il.DeclareLocal(underlyingStronglyTypedIdType);

                il.Emit(OpCodes.Stloc, keyLocal);
                il.Emit(OpCodes.Ldloca, keyLocal);

                il.Emit(OpCodes.Call, underlyingType.GetCastMethod(underlyingType, underlyingStronglyTypedIdType));

                underlyingType = underlyingStronglyTypedIdType;
            }

            if (underlyingType == typeof(ulong))
            {
                underlyingType = typeof(long);

                il.Emit(OpCodes.Ldc_I8, long.MinValue);
                il.Emit(OpCodes.Add);
            }

            if (column.DbType is null)
            {
                il.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(underlyingType).GetConstructor(new[] { stringType, underlyingType }));
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, (int)column.DbType);
                il.Emit(OpCodes.Call, typeof(NpgsqlParameterExtensions).GetMethod("CreateParameter", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(underlyingType));
            }

            il.Emit(OpCodes.Ret);
        }

        private void WriteNullableRetriever(ILGenerator il, ColumnDefinition column, Type underlyingType)
        {
            var stringType = typeof(string);
            var dbNullType = typeof(DBNull);

            var stringConcatMethod = stringType.GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, null, new[] { stringType, stringType }, null);

            var propertyLocal = il.DeclareLocal(column.Property.PropertyType);

            var defaultRetrieverLabel = il.DefineLabel();

            // Check if property has value
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, column.Property.GetGetMethod());
            il.Emit(OpCodes.Stloc_S, propertyLocal);
            il.Emit(OpCodes.Ldloca_S, propertyLocal);
            il.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("HasValue").GetGetMethod());
            il.Emit(OpCodes.Brtrue_S, defaultRetrieverLabel);

            // Nullable retriever
            il.Emit(OpCodes.Ldstr, "@p" + column.Property.Name);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, stringConcatMethod);
            il.Emit(OpCodes.Ldsfld, dbNullType.GetField("Value"));
            il.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(dbNullType).GetConstructor(new[] { stringType, dbNullType }));
            il.Emit(OpCodes.Ret);

            // Default retriever
            il.MarkLabel(defaultRetrieverLabel);

            il.Emit(OpCodes.Ldstr, "@p" + column.Property.Name);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Call, stringConcatMethod);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, column.Property.GetGetMethod());
            il.Emit(OpCodes.Stloc_S, propertyLocal);
            il.Emit(OpCodes.Ldloca_S, propertyLocal);
            il.Emit(OpCodes.Call, propertyLocal.LocalType.GetProperty("Value").GetGetMethod());

            if (typeof(IKey).IsAssignableFrom(underlyingType))
            {
                var underlyingStronglyTypedIdType = underlyingType.GetInterface(typeof(IKey<,>).Name).GetGenericArguments()[1];

                var keyLocal = il.DeclareLocal(underlyingStronglyTypedIdType);

                il.Emit(OpCodes.Stloc, keyLocal);
                il.Emit(OpCodes.Ldloca, keyLocal);

                il.Emit(OpCodes.Call, underlyingType.GetCastMethod(underlyingType, underlyingStronglyTypedIdType));

                underlyingType = underlyingStronglyTypedIdType;
            }

            if (underlyingType == typeof(ulong))
            {
                underlyingType = typeof(long);

                il.Emit(OpCodes.Ldc_I8, long.MinValue);
                il.Emit(OpCodes.Add);
            }

            if (column.DbType is null)
            {
                il.Emit(OpCodes.Newobj, typeof(NpgsqlParameter<>).MakeGenericType(underlyingType).GetConstructor(new[] { stringType, underlyingType }));
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, (int)column.DbType);
                il.Emit(OpCodes.Call, typeof(NpgsqlParameterExtensions).GetMethod("CreateParameter", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(underlyingType));
            }

            il.Emit(OpCodes.Ret);
        }
    }
}
