using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowInsertCommandBuilder<TEntity> : IBaseInsertRelationBuilder<TEntity, TEntity>
        where TEntity : class, new()
    {
        private RelationBuilderValues? _relationBuilderValues;
        private bool _disposeCommand;
        private bool _isFullInsert;
        private bool? _shouldForceLog;

        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly List<(Action<string>, bool)> _loggers;

        internal VenflowInsertCommandBuilder(Database database, Entity<TEntity> entityConfiguration, bool disposeCommand)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _disposeCommand = disposeCommand;

            _command = new();
            _loggers = new(0);
        }

        public IInsertCommand<TEntity> Build()
        {
            var shouldLog = _shouldForceLog ?? _database.DefaultLoggingBehavior == LoggingBehavior.Always || _loggers.Count != 0;

            if (_relationBuilderValues is not null)
            {
                return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand, _relationBuilderValues, _isFullInsert, _loggers, shouldLog);
            }

            return new VenflowInsertCommand<TEntity>(_database, _entityConfiguration, _command, _disposeCommand, _isFullInsert, _loggers, shouldLog);
        }

        public Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entity, cancellationToken);
        }

        public Task<int> InsertAsync(IList<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _disposeCommand = true;

            return Build().InsertAsync(entities, cancellationToken);
        }

        public IBaseInsertRelationBuilder<TEntity, TEntity> WithAll()
        {
            _isFullInsert = true;

            return this;
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.With<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, _entityConfiguration, this, _relationBuilderValues).With(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.With<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, _entityConfiguration, this, _relationBuilderValues).With(propertySelector);
        }

        IInsertRelationBuilder<TToEntity, TEntity> IBaseInsertRelationBuilder<TEntity, TEntity>.With<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector)
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new InsertRelationBuilder<TEntity, TEntity>(_entityConfiguration, _entityConfiguration, this, _relationBuilderValues).With(propertySelector);
        }

        public IBaseInsertRelationBuilder<TEntity, TEntity> Log(bool shouldLog = true)
        {
            _shouldForceLog = shouldLog;

            return this;
        }

        public IBaseInsertRelationBuilder<TEntity, TEntity> LogTo(Action<string> logger, bool includeSensitiveData)
        {
            _loggers.Add((logger, includeSensitiveData));

            return this;
        }

        public IBaseInsertRelationBuilder<TEntity, TEntity> LogTo(params (Action<string> logger, bool includeSensitiveData)[] loggers)
        {
            _loggers.AddRange(loggers);

            return this;
        }
    }
}