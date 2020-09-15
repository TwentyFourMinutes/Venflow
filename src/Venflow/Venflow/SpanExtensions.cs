using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Npgsql.Schema;

namespace Venflow
{
    internal static class SpanExtensions
    {
#if !NET5_0
        private static readonly Dictionary<Type, Delegate> _underylingElementGetter = new();
#endif

        private static readonly Func<ReadOnlyCollection<NpgsqlDbColumn>, List<NpgsqlDbColumn>> _underlyingReadOnlyCollectionListGetter;

        static SpanExtensions()
        {
            _underlyingReadOnlyCollectionListGetter = GetUnderlyingReadOnlyCollectionListGetter();
        }

        internal static Span<T> AsSpan<T>(this List<T> list)
        {
#if NET5_0

            return CollectionsMarshal.AsSpan(list);

#else

            Func<List<T>, T[]> underlyingArrayGetter;

            lock (_underylingElementGetter)
            {
                var genericType = typeof(T[]);

                if (_underylingElementGetter.TryGetValue(genericType, out var tempArrayGetter))
                {
                    underlyingArrayGetter = (Func<List<T>, T[]>)tempArrayGetter;
                }
                else
                {
                    var method = new DynamicMethod("GetUnderlyingArray", genericType, new[] { typeof(List<T>) }, typeof(List<T>));

                    var ilGenerator = method.GetILGenerator();

                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
                    ilGenerator.Emit(OpCodes.Ret);

                    underlyingArrayGetter = (Func<List<T>, T[]>)method.CreateDelegate(typeof(Func<List<T>, T[]>));

                    _underylingElementGetter.Add(genericType, underlyingArrayGetter);
                }
            }

            return underlyingArrayGetter.Invoke(list).AsSpan(0, list.Count);

#endif
        }

        internal static Span<NpgsqlDbColumn> AsSpan(this ReadOnlyCollection<NpgsqlDbColumn> collection)
        {
            return _underlyingReadOnlyCollectionListGetter.Invoke(collection).AsSpan();
        }

        private static Func<ReadOnlyCollection<NpgsqlDbColumn>, List<NpgsqlDbColumn>> GetUnderlyingReadOnlyCollectionListGetter()
        {
            var method = new DynamicMethod("GetUnderlyingList", typeof(List<NpgsqlDbColumn>), new[] { typeof(ReadOnlyCollection<NpgsqlDbColumn>) }, typeof(ReadOnlyCollection<NpgsqlDbColumn>));

            var ilGenerator = method.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, typeof(ReadOnlyCollection<NpgsqlDbColumn>).GetField("list", BindingFlags.NonPublic | BindingFlags.Instance));
            ilGenerator.Emit(OpCodes.Ret);

            return (Func<ReadOnlyCollection<NpgsqlDbColumn>, List<NpgsqlDbColumn>>)method.CreateDelegate(typeof(Func<ReadOnlyCollection<NpgsqlDbColumn>, List<NpgsqlDbColumn>>));

        }
    }
}