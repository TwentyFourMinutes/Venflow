using Npgsql;

namespace Venflow
{
    internal class UInt64Handler : IParameterTypeHandler
    {
        NpgsqlParameter IParameterTypeHandler.Handle(string name, object val)
            => new NpgsqlParameter<long>(name, unchecked((long)val + long.MinValue));
    }
}
