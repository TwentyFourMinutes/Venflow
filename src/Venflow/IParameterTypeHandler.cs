using Npgsql;

namespace Venflow
{
    /// <summary>
    /// A parameter type handler which can be used with the <see cref="ParameterTypeHandler.AddTypeHandler(System.Type, IParameterTypeHandler)"/> method.
    /// </summary>
    public interface IParameterTypeHandler
    {
        /// <summary>
        /// Is used to convert the given value and name to an <see cref="NpgsqlParameter"/>.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="val">The value of the parameter.</param>
        /// <returns>The <see cref="NpgsqlParameter"/> containing the name and the value.</returns>
        NpgsqlParameter Handle(string name, object val);
    }
}
