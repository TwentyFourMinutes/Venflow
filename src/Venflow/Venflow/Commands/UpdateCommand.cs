using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    public class UpdateCommand<TEntity> : VenflowCommand<TEntity> where TEntity : class
    {
        internal UpdateCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity) : base(underlyingCommand, entity)
        {

        }
    }
}
