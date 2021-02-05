using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommand<TEntity, TReturn> : VenflowBaseCommand<TEntity>, IQueryCommand<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        internal Delegate? Materializer { get; set; }

        private readonly RelationBuilderValues? _relationBuilderValues;
        private readonly bool _isSingleResult;
        private readonly List<(Action<string> logger, bool includeSensitiveData)> _loggers;
        private readonly bool _shouldLog;
        private readonly SqlQueryCacheKey _sqlQueryCacheKey;

        internal VenflowQueryCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, RelationBuilderValues? relationBuilderValues, bool trackChanges, bool disposeCommand, bool isSingleResult, List<(Action<string> logger, bool includeSensitiveData)> loggers, bool shouldLog) : base(database, entityConfiguration, underlyingCommand, disposeCommand)
        {
            _relationBuilderValues = relationBuilderValues;
            _isSingleResult = isSingleResult;
            _loggers = loggers;
            _shouldLog = shouldLog;

            underlyingCommand.Connection = database.GetConnection();

            _sqlQueryCacheKey = new SqlQueryCacheKey(underlyingCommand.CommandText, trackChanges);
        }

        async Task<TReturn?> IQueryCommand<TEntity, TReturn>.QueryAsync(CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            if (_shouldLog)
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

            var reader = default(NpgsqlDataReader);

            try
            {
                reader = await UnderlyingCommand.ExecuteReaderAsync(_isSingleResult ? CommandBehavior.SingleRow : CommandBehavior.Default, cancellationToken);

                Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> materializer;

                if (Materializer is { })
                {
                    materializer = (Materializer as Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)!;
                }
                else
                {
                    Materializer = materializer = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer<TReturn>(_relationBuilderValues, reader, _sqlQueryCacheKey);
                }

                return await materializer(reader, cancellationToken);
            }
            finally
            {
                if (reader is not null)
                    await reader.DisposeAsync();

                if (DisposeCommand)
                    await this.DisposeAsync();
            }
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
