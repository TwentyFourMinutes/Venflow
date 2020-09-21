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

namespace Venflow.Dynamic
{
    internal static class SpanExtensions
    {
#if !NET5_0

        private static class SpanMethodCache<TType>
        {
            internal static Func<List<TType>, TType[]> UnderlyingElementGetter;

            static SpanMethodCache()
            {
                var genericType = typeof(TType[]);

                var method = new DynamicMethod("GetUnderlyingArray", genericType, new[] { typeof(List<TType>) }, typeof(List<TType>), true);

                var ilGenerator = method.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, typeof(List<TType>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance));
                ilGenerator.Emit(OpCodes.Ret);

                UnderlyingElementGetter = (Func<List<TType>, TType[]>)method.CreateDelegate(typeof(Func<List<TType>, TType[]>));
            }

            internal static Span<TType> AsSpan(List<TType> list)
            {
                return UnderlyingElementGetter.Invoke(list).AsSpan(0, list.Count);
            }
        }

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

            return SpanMethodCache<T>.AsSpan(list);

#endif
        }

        internal static Span<NpgsqlDbColumn> AsSpan(this ReadOnlyCollection<NpgsqlDbColumn> collection)
        {
            return _underlyingReadOnlyCollectionListGetter.Invoke(collection).AsSpan();
        }

        internal static List<NpgsqlDbColumn> AsList(this ReadOnlyCollection<NpgsqlDbColumn> collection)
        {
            return _underlyingReadOnlyCollectionListGetter.Invoke(collection);
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