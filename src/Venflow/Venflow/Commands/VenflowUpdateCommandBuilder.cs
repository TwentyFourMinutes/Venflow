using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowUpdateCommandBuilder<TEntity> : IUpdateCommandBuilder<TEntity>
        where TEntity : class, new()
    {
        private bool _disposeCommand;
        private bool? _shouldForceLog;

        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly List<LoggerCallback> _loggers;

        internal VenflowUpdateCommandBuilder(Database database, Entity<TEntity> entityConfiguration, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _disposeCommand = disposeCommand;

            _loggers = new(0);
        }

        public IUpdateCommand<TEntity> Build()
        {
            var shouldLog = _shouldForceLog ?? _database.DefaultLoggingBehavior == LoggingBehavior.Always || _loggers.Count != 0;

            return new VenflowUpdateCommand<TEntity>(_database, _entityConfiguration, _disposeCommand, _loggers, shouldLog);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entity, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(List<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(TEntity[] entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }

        ValueTask IUpdateCommandBuilder<TEntity>.UpdateAsync(IList<TEntity> entities, CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().UpdateAsync(entities, cancellationToken);
        }

        public IUpdateCommandBuilder<TEntity> Log(bool shouldLog = true)
        {
            _shouldForceLog = shouldLog;

            return this;
        }

        public IUpdateCommandBuilder<TEntity> LogTo(LoggerCallback logger)
        {
            _loggers.Add(logger);

            return this;
        }

        public IUpdateCommandBuilder<TEntity> LogTo(params LoggerCallback[] loggers)
        {
            _loggers.AddRange(loggers);

            return this;
        }
    }
}