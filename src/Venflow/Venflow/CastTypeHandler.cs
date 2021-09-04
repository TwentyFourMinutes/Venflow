using Npgsql;

namespace Venflow
{
    internal class CastTypeHandler<T> : IParameterTypeHandler
    {
        NpgsqlParameter IParameterTypeHandler.Handle(string name, object val)
            => new NpgsqlParameter<T>(name, (T)val);
    }
}
