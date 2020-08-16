using Npgsql;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommand<TEntity, TReturn> : VenflowBaseCommand<TEntity>, IQueryCommand<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        internal Delegate? Materializer { get; set; }

        private readonly JoinBuilderValues _joinBuilderValues;
        private readonly bool _trackingChanges;
        private readonly bool _isSingleResult;

        internal VenflowQueryCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, JoinBuilderValues joinBuilderValues, bool trackingChanges, bool disposeCommand, bool isSingleResult) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _joinBuilderValues = joinBuilderValues;
            _trackingChanges = trackingChanges;
            _isSingleResult = isSingleResult;
        }

        async Task<TReturn> IQueryCommand<TEntity, TReturn>.QueryAsync(CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            await using var reader = await UnderlyingCommand.ExecuteReaderAsync(_isSingleResult ? CommandBehavior.SingleRow : CommandBehavior.Default, cancellationToken);

            Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> materializer;

            if (Materializer is { })
            {
                materializer = (Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)Materializer;
            }
            else
            {
                Materializer = materializer = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer<TReturn>(_joinBuilderValues, reader.GetColumnSchema(), _trackingChanges);
            }

            var entities = await materializer(reader, cancellationToken);

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
