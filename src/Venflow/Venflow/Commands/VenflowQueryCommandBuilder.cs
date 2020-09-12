using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommandBuilder<TEntity, TReturn> : IPreCommandBuilder<TEntity, TReturn> where TEntity : class, new() where TReturn : class, new()
    {
        internal JoinBuilderValues? JoinBuilderValues { get; set; }

        private bool _trackChanges;
        private QueryGenerationOptions _queryGenerationOptions;
        private bool _disposeCommand;

        private readonly bool _singleResult;
        private readonly ulong _count;
        private readonly StringBuilder _commandString;
        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly object?[]? _interploatedSqlParameters;

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, QueryGenerationOptions queryGenerationOptions, bool disposeCommand, bool singleResult)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _queryGenerationOptions = queryGenerationOptions;
            _command = command;
            _disposeCommand = disposeCommand;
            _singleResult = singleResult;
            _commandString = new StringBuilder();
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, ulong count, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, command, QueryGenerationOptions.GenerateFullSQL, disposeCommand, singleResult)
        {
            _count = count;
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, string sql, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, command, QueryGenerationOptions.None, disposeCommand, singleResult)
        {
            _commandString.Append(sql);
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, FormattableString interpolatedSql, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, command, QueryGenerationOptions.None, disposeCommand, singleResult)
        {
            _interploatedSqlParameters = interpolatedSql.GetArguments();
            _commandString.Append(interpolatedSql.Format);
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, NpgsqlCommand command, string sql, IList<NpgsqlParameter> parameters, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, command, sql, disposeCommand, singleResult)
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                _command.Parameters.Add(parameters[i]);
            }
        }

        JoinBuilder<TEntity, TToEntity, TReturn> IQueryCommandBuilder<TEntity, TReturn>.JoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour)
        {
            var builder = new JoinBuilder<TEntity, TToEntity, TReturn>(_entityConfiguration, this, (_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0);

            return builder.JoinWith(propertySelector, joinBehaviour);
        }

        JoinBuilder<TEntity, TToEntity, TReturn> IQueryCommandBuilder<TEntity, TReturn>.JoinWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour)
        {
            var builder = new JoinBuilder<TEntity, TToEntity, TReturn>(_entityConfiguration, this, (_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0);

            return builder.JoinWith(propertySelector, joinBehaviour);
        }

        JoinBuilder<TEntity, TToEntity, TReturn> IQueryCommandBuilder<TEntity, TReturn>.JoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour)
        {
            var builder = new JoinBuilder<TEntity, TToEntity, TReturn>(_entityConfiguration, this, (_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0);

            return builder.JoinWith(propertySelector, joinBehaviour);
        }

        public IQueryCommandBuilder<TEntity, TReturn> TrackChanges(bool trackChanges = true)
        {
            _trackChanges = trackChanges;

            return this;
        }

        IQueryCommandBuilder<TEntity, TReturn> IPreCommandBuilder<TEntity, TReturn>.AddFormatter()
        {
            _queryGenerationOptions |= QueryGenerationOptions.GenerateJoins;

            return this;
        }

        public IQueryCommand<TEntity, TReturn> Build()
        {
            if (_interploatedSqlParameters is null)
            {
                if ((_queryGenerationOptions & QueryGenerationOptions.GenerateFullSQL) == QueryGenerationOptions.GenerateFullSQL)
                {
                    if (JoinBuilderValues is null)
                    {
                        AppendBaseQuery(_commandString, _count);
                        _commandString.Append(';');
                    }
                    else
                    {
                        BuildRelationQuery(_count);
                    }
                }
                else if ((_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0 &&
                          JoinBuilderValues is { })
                {
                    for (int i = 0; i < _commandString.Length;)
                    {
                        if (_commandString[i] != '>' || i + 1 >= _commandString.Length || _commandString[i + 1] != '<')
                        {
                            i++;
                            continue;
                        }

                        var joinBuilder = new StringBuilder();

                        JoinBuilderValues.AppendColumnNamesAndJoins(null, joinBuilder);

                        _commandString.Remove(i, 2);
                        _commandString.Insert(i, joinBuilder.ToString());

                        break;
                    }
                }
            }
            else
            {
                var generateJoins = (_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0 && JoinBuilderValues is { };

                var parameterCount = 0;

                var gap = generateJoins ? 0 : 2;

                for (int i = 0; i < _commandString.Length - gap; i++)
                {
                    var commandCharacter = _commandString[i];

                    if (commandCharacter == '{')
                    {
                        int digitCount = 0;

                        for (int k = i + 1; k < _commandString.Length; k++)
                        {
                            var character = _commandString[k];

                            if (!char.IsDigit(character))
                            {
                                break;
                            }
                            else
                            {
                                digitCount++;
                            }
                        }

                        if (digitCount == 0)
                            continue;

                        var parameterName = "@p" + parameterCount;

                        _commandString.Remove(i, digitCount + 2);
                        _commandString.Insert(i, parameterName);

                        _command.Parameters.Add(new NpgsqlParameter(parameterName, _interploatedSqlParameters[parameterCount++]));

                        i += parameterName.Length - 1;
                    }
                    else if (generateJoins && commandCharacter == '>' && i + 1 < _commandString.Length && _commandString[i + 1] == '<')
                    {
                        generateJoins = false;

                        var joinBuilder = new StringBuilder();

                        JoinBuilderValues.AppendColumnNamesAndJoins(null, joinBuilder);

                        var joins = joinBuilder.ToString();

                        _commandString.Remove(i, 2);
                        _commandString.Insert(i, joins);

                        i += joins.Length - 1;
                    }
                }
            }

            _command.CommandText = _commandString.ToString();

            return new VenflowQueryCommand<TEntity, TReturn>(_database, _entityConfiguration, _command, JoinBuilderValues, _trackChanges, _disposeCommand, _singleResult && JoinBuilderValues is null);
        }

        private void BuildRelationQuery(ulong count)
        {
            _commandString.Append("SELECT ");

            _commandString.Append(_entityConfiguration.ExplicitColumnListString);

            var subQuery = new StringBuilder();

            subQuery.Append(" FROM (");

            AppendBaseQuery(subQuery, count);

            subQuery.Append(") AS ");

            subQuery.Append(_entityConfiguration.TableName);

            JoinBuilderValues!.AppendColumnNamesAndJoins(_commandString, subQuery);

            _commandString.Append(subQuery);

            _commandString.Append(';');
        }

        private void AppendBaseQuery(StringBuilder sb, ulong count)
        {
            sb.Append("SELECT ");

            sb.Append(_entityConfiguration.ExplicitColumnListString);

            sb.Append(" FROM ");

            sb.Append(_entityConfiguration.TableName);

            if (count > 0)
            {
                sb.Append(" LIMIT ");
                sb.Append(count);
            }
        }

#if !NET48
        [return: MaybeNull]
#endif
        Task<TReturn> IQueryCommandBuilder<TEntity, TReturn>.QueryAsync(CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().QueryAsync(cancellationToken);
        }

#if !NET48
        [return: MaybeNull]
#endif
        Task<TReturn> IPreCommandBuilder<TEntity, TReturn>.QueryAsync(CancellationToken cancellationToken)
        {
            _disposeCommand = true;

            return Build().QueryAsync(cancellationToken);
        }
    }
}