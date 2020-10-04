using System;
using System.Runtime.CompilerServices;
using Npgsql;

namespace Venflow
{

    public static class ParameterTypeHandler
    {
        private readonly static ConditionalWeakTable<Type, IParameterTypeHandler> _typeHandlers = new ConditionalWeakTable<Type, IParameterTypeHandler>();

        static ParameterTypeHandler()
        {
            var uInt64Handler = new UInt64Handler();
            AddTypeHandler(typeof(ulong), uInt64Handler);
            AddTypeHandler(typeof(ulong?), uInt64Handler);
        }

        public static void AddTypeHandler(Type type, IParameterTypeHandler typeHandler)
           => _typeHandlers.Add(type, typeHandler);

        internal static NpgsqlParameter HandleParameter(string name, object? val)
        {
            if (val is null)
            {
                return new NpgsqlParameter(name, DBNull.Value);
            }

            if (!_typeHandlers.TryGetValue(val.GetType(), out var handler))
                return new NpgsqlParameter(name, val);

            return handler.Handle(name, val);
        }
    }
}
