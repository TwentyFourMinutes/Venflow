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
            switch (val)
            {
                case null:
                    return new NpgsqlParameter(name, DBNull.Value);
                case IKey key:
                    val = key.BoxedValue;
                    break;
            }

            if (!_typeHandlers.TryGetValue(val.GetType(), out var handler))
                return new NpgsqlParameter(name, val);

            return handler.Handle(name, val);
        }

        internal static NpgsqlParameter HandleParameter<T>(string name, T? val)
        {
            IParameterTypeHandler? handler;

            switch (val)
            {
                case null:
                    return new NpgsqlParameter<DBNull>(name, DBNull.Value);

                case IKey key:
                    var tempVal = key.BoxedValue;

                    if (!_typeHandlers.TryGetValue(tempVal.GetType(), out handler))
                        return new NpgsqlParameter(name, tempVal);

                    return handler.Handle(name, tempVal);
            }

            if (!_typeHandlers.TryGetValue(val.GetType(), out handler))
                return new NpgsqlParameter<T>(name, val);

            return handler.Handle(name, val);
        }

        internal static NpgsqlParameter HandleParameter(string name, Type type, object? val)
        {
            switch (val)
            {
                case null:
                    return new NpgsqlParameter(name, DBNull.Value);
                case IKey key:
                    val = key.BoxedValue;
                    break;
            }

            if (!_typeHandlers.TryGetValue(type, out var handler))
                return new NpgsqlParameter(name, val);

            return handler.Handle(name, val);
        }
    }
}
