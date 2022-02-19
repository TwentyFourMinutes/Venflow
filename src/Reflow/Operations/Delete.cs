using System.Data.Common;
using Npgsql;

namespace Reflow.Operations
{
    internal static class Delete
    {
        internal static async Task<int> DeleteAsync<TEntity>(
            IDatabase database,
            TEntity entity,
            CancellationToken cancellationToken
        )
        {
            var command = (DbCommand)new NpgsqlCommand();
            command.Connection = database.Connection;

            if (entity is null)
                return default;

            try
            {
                (
                    (Action<DbCommand, TEntity>)database.Configuration.SingleDeletes[
                        typeof(TEntity)
                    ]
                ).Invoke(command, entity);

                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await command.DisposeAsync();
            }
        }

        internal static async Task<int> DeleteAsync<TEntity>(
            IDatabase database,
            IList<TEntity> entities,
            CancellationToken cancellationToken
        )
        {
            var command = (DbCommand)new NpgsqlCommand();
            command.Connection = database.Connection;

            if (entities is null || entities.Count is 0)
                return default;

            try
            {
                (
                    (Action<DbCommand, IList<TEntity>>)database.Configuration.ManyDeletes[
                        typeof(TEntity)
                    ]
                ).Invoke(command, entities);

                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await command.DisposeAsync();
            }
        }
    }
}
