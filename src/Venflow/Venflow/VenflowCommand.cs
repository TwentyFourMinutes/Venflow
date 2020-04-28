using Npgsql;
using System;
using System.Threading.Tasks;

namespace Venflow
{
    public class VenflowCommand<TEntity> : IAsyncDisposable where TEntity : class
    {
        public NpgsqlCommand UnderlyingCommand { get; set; }

        internal Entity<TEntity> EntityConfiguration { get; set; }

        internal bool GetInstertedId { get; set; }
        internal bool OrderPreservedColumns { get; set; }

        internal VenflowCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity)
        {
            UnderlyingCommand = underlyingCommand;
            EntityConfiguration = entity;
        }

        public ValueTask DisposeAsync()
        {
            return UnderlyingCommand.DisposeAsync();
        }
    }
}
