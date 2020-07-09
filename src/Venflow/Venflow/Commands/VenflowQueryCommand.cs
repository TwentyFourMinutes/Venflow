﻿using Npgsql;
using System;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommand<TEntity, TReturn> : VenflowBaseCommand<TEntity>, IQueryCommand<TEntity, TReturn> where TEntity : class where TReturn : class
    {
        internal Delegate? Materializer { get; set; }

        private readonly JoinBuilderValues _joinBuilderValues;
        private readonly bool _trackingChanges;

        internal VenflowQueryCommand(DbConfiguration dbConfiguration, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, JoinBuilderValues joinBuilderValues, bool trackingChanges, bool disposeCommand) : base(dbConfiguration, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _joinBuilderValues = joinBuilderValues;
            _trackingChanges = trackingChanges;
        }

        async Task<TReturn> IQueryCommand<TEntity, TReturn>.QueryAsync(CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            await using var reader = await UnderlyingCommand.ExecuteReaderAsync(cancellationToken);

            Func<NpgsqlDataReader, Task<TReturn>> materializer;

            if (Materializer is { })
            {
                materializer = (Func<NpgsqlDataReader, Task<TReturn>>)Materializer;
            }
            else
            {
                Materializer = materializer = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer<TReturn>(_joinBuilderValues, DbConfiguration, reader.GetColumnSchema(), _trackingChanges);
            }

            var entities = await materializer(reader);

            if (DisposeCommand)
                await this.DisposeAsync();

            return entities;
        }

        async Task<IQueryCommand<TEntity, TReturn>> IQueryCommand<TEntity, TReturn>.PrepareAsync(CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            await UnderlyingCommand.PrepareAsync(cancellationToken);

            return this;
        }

        async Task<IQueryCommand<TEntity, TReturn>> IQueryCommand<TEntity, TReturn>.UnprepareAsync(CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            await UnderlyingCommand.UnprepareAsync(cancellationToken);

            return this;
        }

        public async ValueTask DisposeAsync()
        {
            UnderlyingCommand.Dispose();

            if (UnderlyingCommand.IsPrepared)
            {
                await UnderlyingCommand.UnprepareAsync();
            }
        }
    }
}