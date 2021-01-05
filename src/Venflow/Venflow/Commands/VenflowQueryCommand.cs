using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommand<TEntity, TReturn> : VenflowBaseCommand<TEntity>, IQueryCommand<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        internal Delegate? Materializer { get; set; }

        private readonly RelationBuilderValues? _relationBuilderValues;
        private readonly bool _trackingChanges;
        private readonly bool _isSingleResult;
        private readonly List<(Action<string> logger, bool includeSensitiveData)> _loggers;
        private readonly bool? _shouldForceLog;

        internal VenflowQueryCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, RelationBuilderValues? relationBuilderValues, bool trackingChanges, bool disposeCommand, bool isSingleResult, List<(Action<string> logger, bool includeSensitiveData)> loggers, bool? shouldForceLog) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _relationBuilderValues = relationBuilderValues;
            _trackingChanges = trackingChanges;
            _isSingleResult = isSingleResult;
            _loggers = loggers;
            _shouldForceLog = shouldForceLog;
        }

        async Task<TReturn> IQueryCommand<TEntity, TReturn>.QueryAsync(CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            if (Database.DefaultLoggingBehavior == LoggingBehavior.Always && (!_shouldForceLog.HasValue || _shouldForceLog.Value) ||
                (_shouldForceLog.HasValue && _shouldForceLog.Value))
            {
                if (_loggers.Count == 0)
                {
                    Database.ExecuteLoggers(UnderlyingCommand);
                }
                else
                {
                    Database.ExecuteLoggers(_loggers, UnderlyingCommand);
                }
            }

            await using var reader = await UnderlyingCommand.ExecuteReaderAsync(_isSingleResult ? CommandBehavior.SingleRow : CommandBehavior.Default, cancellationToken);

            Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> materializer;

            if (Materializer is { })
            {
                materializer = (Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)Materializer;
            }
            else
            {
                Materializer = materializer = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer<TReturn>(_relationBuilderValues, reader.GetColumnSchema(), _trackingChanges);
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
