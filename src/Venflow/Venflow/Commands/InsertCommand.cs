using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    public class InsertCommand<TEntity> : VenflowCommand<TEntity> where TEntity : class
    {
        public bool GetInsertedId { get; set; }

        internal int StartIndex { get; set; }

        internal InsertCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity) : base(underlyingCommand, entity)
        {

        }
    }
}
