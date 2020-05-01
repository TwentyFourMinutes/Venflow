using Npgsql;
using System;
using Venflow.Modeling;

namespace Venflow.Commands
{
    public abstract class VenflowCommand<TEntity> : IDisposable where TEntity : class
    {
        public NpgsqlCommand UnderlyingCommand { get; set; }

        internal Entity<TEntity> EntityConfiguration { get; set; }

        private protected VenflowCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity)
        {
            UnderlyingCommand = underlyingCommand;
            EntityConfiguration = entity;
        }

        public void Dispose()
        {
            UnderlyingCommand.Dispose();
        }
    }
}
