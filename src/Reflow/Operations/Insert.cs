﻿using System.Data.Common;
using Npgsql;

namespace Reflow.Operations
{
    internal static class Insert
    {
        internal static async Task InsertAsync<TEntity>(
            IDatabase database,
            TEntity entity,
            CancellationToken cancellationToken
        )
        {
            var command = (DbCommand)new NpgsqlCommand();
            command.Connection = database.Connection;

            await database.EnsureValidConnection(cancellationToken);

            try
            {
                await (
                    (Func<DbCommand, TEntity, Task>)database.Configuration.SingleInserts[
                        typeof(TEntity)
                    ]
                ).Invoke(command, entity);
            }
            finally
            {
                await command.DisposeAsync();
            }
        }

        internal static async Task InsertAsync<TEntity>(
            IDatabase database,
            IList<TEntity> entities,
            CancellationToken cancellationToken
        )
        {
            var command = (DbCommand)new NpgsqlCommand();
            command.Connection = database.Connection;

            await database.EnsureValidConnection(cancellationToken);

            try
            {
                await (
                    (Func<DbCommand, IList<TEntity>, Task>)database.Configuration.ManyInserts[
                        typeof(TEntity)
                    ]
                ).Invoke(command, entities);
            }
            finally
            {
                await command.DisposeAsync();
            }
        }
    }
}