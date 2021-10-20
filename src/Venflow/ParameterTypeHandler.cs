using Npgsql;
using NpgsqlTypes;

namespace Venflow
{
    /// <summary>
    /// A class which contains methods to configure the used type handlers while parsing Interpolated arguments.
    /// </summary>
    public static class ParameterTypeHandler
    {
        internal static HashSet<Type> PostgreEnums => _postgreEnums;

        private readonly static Dictionary<Type, IParameterTypeHandler> _typeHandlers = new Dictionary<Type, IParameterTypeHandler>();
        private readonly static Dictionary<Type, IParameterTypeHandler> _castHandlers = new Dictionary<Type, IParameterTypeHandler>(1);
        private readonly static HashSet<Type> _postgreEnums = new HashSet<Type>(0);

        static ParameterTypeHandler()
        {
            var uInt64Handler = new UInt64Handler();
            AddTypeHandler(typeof(ulong), uInt64Handler);
            AddTypeHandler(typeof(ulong?), uInt64Handler);

            _castHandlers.Add(typeof(ulong), uInt64Handler);
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
            Type? type = null;
            IParameterTypeHandler? handler;

            switch (val)
            {
                case null:
                    return new NpgsqlParameter(name, DBNull.Value);
                case IKey key:
                    val = key.BoxedValue;
                    break;
                case Enum:
                    type = val.GetType();

                    var tempType = Nullable.GetUnderlyingType(type) ?? type;

                    if (!_postgreEnums.Contains(tempType))
                    {
                        if (!_typeHandlers.TryGetValue(tempType, out handler))
                        {
                            var underlyingType = tempType.GetEnumUnderlyingType();

                            if (!_castHandlers.TryGetValue(underlyingType, out handler))
                            {
                                handler = (IParameterTypeHandler)Activator.CreateInstance(typeof(CastTypeHandler<>).MakeGenericType(underlyingType))!;

                                _castHandlers.Add(underlyingType, handler);
                            }

                            _typeHandlers.Add(tempType, handler);
                        }

                        return handler.Handle(name, val);
                    }
                    break;
            }

            type ??= val!.GetType();

            if (!_typeHandlers.TryGetValue(type, out handler))
                return new NpgsqlParameter(name, val);

            return handler.Handle(name, val!);
        }

        internal static NpgsqlParameter HandleParameter<T>(string name, T? val)
        {
            Type? type = null;
            IParameterTypeHandler? handler;

            switch (val)
            {
                case null:
                    return new NpgsqlParameter<DBNull>(name, DBNull.Value);
                case IKey key:
                    var tempVal = key.BoxedValue;

                    if (!_typeHandlers.TryGetValue(tempVal!.GetType(), out handler))
                        return new NpgsqlParameter(name, tempVal);

                    return handler.Handle(name, tempVal);
                case Enum:
                    type = val.GetType();

                    var tempType = Nullable.GetUnderlyingType(type) ?? type;

                    if (!_postgreEnums.Contains(tempType))
                    {
                        if (!_typeHandlers.TryGetValue(tempType, out handler))
                        {
                            var underlyingType = tempType.GetEnumUnderlyingType();

                            if (!_castHandlers.TryGetValue(underlyingType, out handler))
                            {
                                handler = (IParameterTypeHandler)Activator.CreateInstance(typeof(CastTypeHandler<>).MakeGenericType(underlyingType!))!;

                                _castHandlers.Add(underlyingType, handler!);
                            }

                            _typeHandlers.Add(tempType, handler);
                        }

                        return handler.Handle(name, val);
                    }
                    break;
            }

            type ??= val.GetType();

            if (!_typeHandlers.TryGetValue(type, out handler))
                return new NpgsqlParameter<T>(name, val);

            return handler.Handle(name, val);
        }

        internal static NpgsqlParameter HandleParameter(string name, object? val, NpgsqlDbType dbType)
        {
            if (val is null)
            {
                return new NpgsqlParameter<DBNull>(name, DBNull.Value);
            }

            var parameter = new NpgsqlParameter(name, dbType);

            parameter.Value = val;

            return parameter;
        }

        internal static NpgsqlParameter HandleParameter(string name, Type type, object? val)
        {
            IParameterTypeHandler? handler;

            switch (val)
            {
                case null:
                    return new NpgsqlParameter(name, DBNull.Value);
                case IKey key:
                    val = key.BoxedValue;
                    break;
                case Enum:
                    var tempType = Nullable.GetUnderlyingType(type) ?? type;

                    if (!_postgreEnums.Contains(tempType))
                    {
                        if (!_typeHandlers.TryGetValue(tempType, out handler))
                        {
                            var underlyingType = tempType.GetEnumUnderlyingType();

                            if (!_castHandlers.TryGetValue(underlyingType, out handler))
                            {
                                handler = (IParameterTypeHandler)Activator.CreateInstance(typeof(CastTypeHandler<>).MakeGenericType(underlyingType!))!;

                                _castHandlers.Add(underlyingType, handler!);
                            }

                            _typeHandlers.Add(tempType, handler!);
                        }

                        return handler!.Handle(name, val);
                    }
                    break;
            }

            if (!_typeHandlers.TryGetValue(type, out handler))
                return new NpgsqlParameter(name, val);

            return handler.Handle(name, val!);
        }
    }
}
