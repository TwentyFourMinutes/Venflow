using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Dynamic;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommandBuilder<TEntity, TReturn> : IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        private bool _trackChanges;
        private QueryGenerationOptions _queryGenerationOptions;
        private bool _disposeCommand;

        private RelationBuilderValues? _relationBuilderValues;
        private readonly bool _singleResult;
        private readonly StringBuilder _commandString;
        private readonly string _rawSql;
        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly object?[]? _interploatedSqlParameters;

        private VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, QueryGenerationOptions queryGenerationOptions, bool disposeCommand, bool singleResult)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _queryGenerationOptions = queryGenerationOptions;
            _command = command;
            _disposeCommand = disposeCommand;
            _singleResult = singleResult;
            _commandString = new StringBuilder();
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, string sql, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, command, QueryGenerationOptions.None, disposeCommand, singleResult)
        {
            _rawSql = sql;
            _commandString.Append(_rawSql);
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, FormattableString interpolatedSql, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, command, QueryGenerationOptions.None, disposeCommand, singleResult)
        {
            _interploatedSqlParameters = interpolatedSql.GetArguments();
            _rawSql = interpolatedSql.Format;
            _commandString.Append(_rawSql);
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, string sql, IList<NpgsqlParameter> parameters, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, command, sql, disposeCommand, singleResult)
        {
            for (int i = parameters.Count - 1; i >= 0; i--)
            {
                _command.Parameters.Add(parameters[i]);
            }
        }

        public IQueryCommandBuilder<TEntity, TReturn> TrackChanges(bool trackChanges = true)
        {
            _trackChanges = trackChanges;

            return this;
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> AddFormatter()
        {
            _queryGenerationOptions |= QueryGenerationOptions.GenerateJoins;

            return this;
        }

        public IQueryCommand<TEntity, TReturn> Build()
        {
            if (_interploatedSqlParameters is null)
            {
                if ((_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0 &&
                    _relationBuilderValues is { })
                {
                    var joinBuilder = new StringBuilder();

                    AppendJoins(joinBuilder);

                    _command.CommandText = _rawSql.Replace("><", joinBuilder.ToString());
                }
                else
                {
                    _command.CommandText = _rawSql;
                }
            }
            else
            {
                var argumentsSpan = _interploatedSqlParameters.AsSpan();

                var parameterNames = new string[argumentsSpan.Length];
                var parameterNamesSpan = parameterNames.AsSpan();

                for (int argumentIndex = 0; argumentIndex < argumentsSpan.Length; argumentIndex++)
                {
                    var parameterName = "@p" + argumentIndex;

                    parameterNamesSpan[argumentIndex] = parameterName;

                    _command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, argumentsSpan[argumentIndex]));
                }

                var argumentedSql = string.Format(_rawSql, parameterNames);

                if ((_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0 &&
                     _relationBuilderValues is { })
                {
                    var joinBuilder = new StringBuilder();

                    AppendJoins(joinBuilder);

                    argumentedSql = argumentedSql.Replace("><", joinBuilder.ToString());
                }

                _command.CommandText = argumentedSql;
            }

            return new VenflowQueryCommand<TEntity, TReturn>(_database, _entityConfiguration, _command, _relationBuilderValues, _trackChanges, _disposeCommand, _singleResult && _relationBuilderValues is null);
        }

        private void AppendJoins(StringBuilder sb)
        {
            var relationsSpan = _relationBuilderValues.FlattenedPath.AsSpan();

            for (int max = relationsSpan.Length, current = 0; current < max; current++)
            {
                var relationPath = (RelationPath<JoinBehaviour>)relationsSpan[current];
                var relation = relationPath.CurrentRelation;

                switch (relationPath.Value)
                {
                    case JoinBehaviour.InnerJoin:
                        sb.Append("INNER JOIN ");
                        break;
                    case JoinBehaviour.LeftJoin:
                        sb.Append("LEFT JOIN ");
                        break;
                    case JoinBehaviour.RightJoin:
                        sb.Append("RIGHT JOIN ");
                        break;
                    case JoinBehaviour.FullJoin:
                        sb.Append("FULL JOIN ");
                        break;
                    default:
                        throw new InvalidOperationException($"Invalid state '{relationPath.Value}' for the JoinBehaviour on entity {relation.RightEntity.EntityName}");
                }

                sb.Append(relation.RightEntity.TableName);
                sb.Append(" ON ");

                if (relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                {
                    sb.Append(relation.LeftEntity.TableName);
                    sb.Append(".\"");
                    sb.Append(relation.ForeignKeyColumn.ColumnName);
                    sb.Append("\" = ");
                    sb.Append(relation.RightEntity.TableName);
                    sb.Append(".\"");
                    sb.Append(relation.RightEntity.GetPrimaryColumn().ColumnName);
                }
                else
                {
                    sb.Append(relation.RightEntity.TableName);
                    sb.Append(".\"");
                    sb.Append(relation.ForeignKeyColumn.ColumnName);
                    sb.Append("\" = ");
                    sb.Append(relation.LeftEntity.TableName);
                    sb.Append(".\"");
                    sb.Append(relation.LeftEntity.GetPrimaryColumn().ColumnName);
                }

                sb.Append('"');
            }

        }

#if !NET48
        [return: MaybeNull]
#endif
        public Task<TReturn> QueryAsync(CancellationToken cancellationToken = default)
        {
            _disposeCommand = true;

            return Build().QueryAsync(cancellationToken);
        }

        public IQueryRelationBuilder<TToEntity, TEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new QueryRelationBuilder<TEntity, TEntity, TReturn>(_entityConfiguration, this, _relationBuilderValues).JoinWith(propertySelector, joinBehaviour);
        }

        public IQueryRelationBuilder<TToEntity, TEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new QueryRelationBuilder<TEntity, TEntity, TReturn>(_entityConfiguration, this, _relationBuilderValues).JoinWith(propertySelector, joinBehaviour);
        }

        public IQueryRelationBuilder<TToEntity, TEntity, TReturn> JoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class, new()
        {
            _relationBuilderValues = new RelationBuilderValues(_entityConfiguration);

            return new QueryRelationBuilder<TEntity, TEntity, TReturn>(_entityConfiguration, this, _relationBuilderValues).JoinWith(propertySelector, joinBehaviour);
        }
    }
}