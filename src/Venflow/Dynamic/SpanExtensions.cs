using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Npgsql.Schema;

namespace Venflow.Dynamic
{
    internal static class SpanExtensions
    {
#if !NET5_0_OR_GREATER

        private static class SpanMethodCache<TType>
        {
            internal static Func<List<TType>, TType[]> UnderlyingElementGetter;

            static SpanMethodCache()
            {
                var genericType = typeof(TType[]);

                var method = TypeFactory.GetDynamicMethod("GetUnderlyingArray", genericType, new[] { typeof(List<TType>) });

                var ilGenerator = method.GetILGenerator();

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, typeof(List<TType>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)!);
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
#if NET5_0_OR_GREATER

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
            var method = TypeFactory.GetDynamicMethod("GetUnderlyingList", typeof(List<NpgsqlDbColumn>), new[] { typeof(ReadOnlyCollection<NpgsqlDbColumn>) });

            var ilGenerator = method.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, typeof(ReadOnlyCollection<NpgsqlDbColumn>).GetField("list", BindingFlags.NonPublic | BindingFlags.Instance)!);
            ilGenerator.Emit(OpCodes.Ret);

            return (Func<ReadOnlyCollection<NpgsqlDbColumn>, List<NpgsqlDbColumn>>)method.CreateDelegate(typeof(Func<ReadOnlyCollection<NpgsqlDbColumn>, List<NpgsqlDbColumn>>));

        }
    }
}
