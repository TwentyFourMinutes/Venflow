using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    public class DeleteCommand<TEntity> : VenflowCommand<TEntity> where TEntity : class
    {
        internal DeleteCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity) : base(underlyingCommand, entity)
        {

        }
    }
}
