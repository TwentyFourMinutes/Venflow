using System;
using System.Collections.Generic;
using Venflow.Dynamic.Materializer;
using Venflow.Modeling;

namespace Venflow.Tests
{
    internal static class TableExtensions
    {
        internal static void ClearMaterializerCache<TEntity>(this Table<TEntity> table) where TEntity : class, new()
        {
            var entity = (Entity<TEntity>)typeof(Table<TEntity>).GetField("Configuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(table);

            var factory = entity.MaterializerFactory;

            var cache = (Dictionary<int, Delegate>)typeof(MaterializerFactory<TEntity>).GetField("_materializerCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(factory);

            cache.Clear();
        }
    }
}
