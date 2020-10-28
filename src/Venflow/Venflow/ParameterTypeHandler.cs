using System;
using System.Runtime.CompilerServices;
using Npgsql;

namespace Venflow
{
    /// <summary>
    /// A class which contains methods to configure the used type handlers while parsing Interpolated arguments.
    /// </summary>
    public static class ParameterTypeHandler
    {
        private readonly static ConditionalWeakTable<Type, IParameterTypeHandler> _typeHandlers = new ConditionalWeakTable<Type, IParameterTypeHandler>();

        static ParameterTypeHandler()
        {
            var uInt64Handler = new UInt64Handler();
            AddTypeHandler(typeof(ulong), uInt64Handler);
            AddTypeHandler(typeof(ulong?), uInt64Handler);
        }

        /// <summary>
        /// Adds a type handler.
        /// </summary>
        /// <param name="type">The type to which the type handler should be mapped.</param>
        /// <param name="typeHandler">The type handler.</param>
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
