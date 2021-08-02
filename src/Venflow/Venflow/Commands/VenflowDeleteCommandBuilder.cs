using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowDeleteCommandBuilder<TEntity> : IDeleteCommandBuilder<TEntity>
        where TEntity : class, new()
    {
        private bool _disposeCommand;
        private bool? _shouldForceLog;

        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly List<LoggerCallback> _loggers;

        internal VenflowDeleteCommandBuilder(Database database, Entity<TEntity> entityConfiguration, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _disposeCommand = disposeCommand;

            _loggers = new(0);
        }

        public IDeleteCommand<TEntity> Build()
        {
            var shouldLog = _shouldForceLog ?? _database.DefaultLoggingBehavior == LoggingBehavior.Always || _loggers.Count != 0;

            return new VenflowDeleteCommand<TEntity>(_database, _entityConfiguration, _disposeCommand, _loggers, shouldLog);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entity, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }

        ValueTask<int> IDeleteCommandBuilder<TEntity>.DeleteAsync(TEntity[] entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().DeleteAsync(entities, cancellationToken);
        }

        public IDeleteCommandBuilder<TEntity> Log(bool shouldLog = true)
        {
            _shouldForceLog = shouldLog;

            return this;
        }

        public IDeleteCommandBuilder<TEntity> LogTo(LoggerCallback logger)
        {
            _loggers.Add(logger);

            return this;
        }

        public IDeleteCommandBuilder<TEntity> LogTo(params LoggerCallback[] loggers)
        {
            _loggers.AddRange(loggers);

            return this;
        }
    }
}