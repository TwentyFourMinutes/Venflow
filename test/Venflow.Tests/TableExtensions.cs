﻿using System.Collections;
using Venflow.Dynamic.Materializer;
using Venflow.Modeling;

namespace Venflow.Tests
{
    internal static class TableExtensions
    {
        internal static void ClearMaterializerCache<TEntity>(this Table<TEntity> table) where TEntity : class, new()
        {
            var entity = (Entity<TEntity>)typeof(Table<TEntity>).GetProperty("Configuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetGetMethod(true).Invoke(table, null);

            var factory = entity.MaterializerFactory;

            var materializerCache = (IDictionary)typeof(MaterializerFactory<TEntity>).GetField("_materializerCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(factory);
            var primaryCache = (IDictionary)typeof(MaterializerFactory<TEntity>).GetField("_primaryMaterializerCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(factory);

            var expirationField = typeof(MaterializerFactory<TEntity>).GetField("_primaryExpirations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var expirationCache = expirationField.GetValue(factory);

            var materializerLock = typeof(MaterializerFactory<TEntity>).GetField("_materializerLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(factory);

            lock (materializerLock)
            {
                materializerCache.Clear();
                primaryCache.Clear();
                expirationField.FieldType.GetMethod("Clear").Invoke(expirationCache, null);
            }
        }
    }
}
