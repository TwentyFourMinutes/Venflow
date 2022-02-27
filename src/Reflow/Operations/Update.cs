using System.Data.Common;
using Npgsql;

namespace Reflow.Operations
{
    internal static class Update
    {
        internal static async Task<int> UpdateAsync<TEntity>(
            IDatabase database,
            TEntity entity,
            CancellationToken cancellationToken
        )
        {
            var command = (DbCommand)new NpgsqlCommand();
            command.Connection = database.Connection;

            try
            {
                (
                    (Action<DbCommand, TEntity>)database.Configuration.SingleUpdates[
                        typeof(TEntity)
                    ]
                ).Invoke(command, entity);

                if (command.Parameters.Count == 0)
                    return 0;

                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await command.DisposeAsync();
            }
        }

        internal static async Task<int> UpdateAsync<TEntity>(
            IDatabase database,
            IList<TEntity> entities,
            CancellationToken cancellationToken
        )
        {
            var command = (DbCommand)new NpgsqlCommand();
            command.Connection = database.Connection;

            try
            {
                (
                    (Action<DbCommand, IList<TEntity>>)database.Configuration.ManyUpdates[
                        typeof(TEntity)
                    ]
                ).Invoke(command, entities);

                if (command.Parameters.Count == 0)
                    return 0;

                return await command.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await command.DisposeAsync();
            }
        }
    }
}
