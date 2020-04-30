using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Venflow
{
    public class VenflowCommand<TEntity> : IDisposable where TEntity : class
    {
        public NpgsqlCommand UnderlyingCommand { get; set; }

        internal Entity<TEntity> EntityConfiguration { get; set; }

        internal Func<NpgsqlDataReader, TEntity>? EntityFactory { get; set; }

        internal bool GetInstertedId { get; set; }
        internal bool OrderPreservedColumns { get; set; }

        internal VenflowCommand(NpgsqlCommand underlyingCommand, Entity<TEntity> entity)
        {
            UnderlyingCommand = underlyingCommand;
            EntityConfiguration = entity;
        }

        public Task<PreparedCommand<TEntity>> PrepareSelfAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default)
        {
            return PreparedCommand<TEntity>.CreateAsync(connection, this, cancellationToken);
        }

        public Task<PreparedCommand<TEntity>> PrepareSelfAsync(VenflowDbConnection connection, CancellationToken cancellationToken = default)
        {
            return PreparedCommand<TEntity>.CreateAsync(connection.Connection, this, cancellationToken);
        }

        public void Dispose()
        {
            UnderlyingCommand.Dispose();
        }
    }
}
