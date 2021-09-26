using Npgsql;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommand<TEntity, TReturn> : VenflowBaseCommand<TEntity>, IQueryCommand<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        private Delegate? _materializer;

        private readonly RelationBuilderValues? _relationBuilderValues;
        private readonly bool _isSingleResult;
        private readonly SqlQueryCacheKey _sqlQueryCacheKey;

        internal VenflowQueryCommand(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand underlyingCommand, RelationBuilderValues? relationBuilderValues, bool trackChanges, bool disposeCommand, bool isSingleResult, List<LoggerCallback> loggers, bool shouldLog) : base(database, entityConfiguration, underlyingCommand, disposeCommand, loggers, shouldLog)
        {
            _relationBuilderValues = relationBuilderValues;
            _isSingleResult = isSingleResult;

            _sqlQueryCacheKey = new SqlQueryCacheKey(underlyingCommand.CommandText, trackChanges, typeof(TReturn));
        }

        async Task<TReturn?> IQueryCommand<TEntity, TReturn>.QueryAsync(CancellationToken cancellationToken)
        {
            await ValidateConnectionAsync();

            var reader = default(NpgsqlDataReader);

            try
            {
                reader = await UnderlyingCommand.ExecuteReaderAsync(_isSingleResult ? System.Data.CommandBehavior.SingleRow : System.Data.CommandBehavior.Default, cancellationToken);

                Func<NpgsqlDataReader, CancellationToken, Task<TReturn>> materializer;

                if (_materializer is not null)
                {
                    materializer = (_materializer as Func<NpgsqlDataReader, CancellationToken, Task<TReturn>>)!;
                }
                else
                {
                    _materializer = materializer = EntityConfiguration.MaterializerFactory.GetOrCreateMaterializer<TReturn>(_relationBuilderValues, reader, _sqlQueryCacheKey);
                }

                var result = await materializer(reader, cancellationToken);

                Log(_isSingleResult ? Enums.CommandType.QuerySingle : Enums.CommandType.QueryBatch);

                return result;
            }
            catch (Exception ex) when (Log(_isSingleResult ? Enums.CommandType.QuerySingle : Enums.CommandType.QueryBatch, ex))
            {
                return default;
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
