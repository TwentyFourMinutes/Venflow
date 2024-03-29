using System.Linq.Expressions;
using Npgsql;
using NpgsqlTypes;
using Venflow.Dynamic;
using Venflow.Dynamic.Materializer;
using Venflow.Enums;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowQueryCommandBuilder<TEntity, TReturn> : IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>
        where TEntity : class, new()
        where TReturn : class, new()
    {
        private bool _trackChanges;
        private QueryGenerationOptions _queryGenerationOptions;
        private bool _disposeCommand;
        private bool? _shouldForceLog;

        private RelationBuilderValues? _relationBuilderValues;
        private readonly string _rawSql = null!;
        private readonly bool _singleResult;
        private readonly NpgsqlCommand _command;
        private readonly Database _database;
        private readonly Entity<TEntity> _entityConfiguration;
        private readonly object?[]? _interpolatedSqlParameters;
        private readonly LambdaExpression? _interpolatedSqlExpression;
        private readonly List<LoggerCallback> _loggers;

        private VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, QueryGenerationOptions queryGenerationOptions, bool disposeCommand, bool singleResult)
        {
            _database = database;
            _entityConfiguration = entityConfiguration;
            _queryGenerationOptions = queryGenerationOptions;
            _disposeCommand = disposeCommand;
            _singleResult = singleResult;

            _loggers = new(0);
            _command = new();
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, string sql, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, QueryGenerationOptions.None, disposeCommand, singleResult)
        {
            _rawSql = sql;
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, LambdaExpression interpolatedSqlExpression, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, QueryGenerationOptions.None, disposeCommand, singleResult)
        {
            _interpolatedSqlExpression = interpolatedSqlExpression;
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, FormattableString interpolatedSql, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, QueryGenerationOptions.None, disposeCommand, singleResult)
        {
            _interpolatedSqlParameters = interpolatedSql.GetArguments();
            _rawSql = interpolatedSql.Format;
        }

        internal VenflowQueryCommandBuilder(Database database, Entity<TEntity> entityConfiguration, string sql, IList<NpgsqlParameter> parameters, bool disposeCommand, bool singleResult) : this(database, entityConfiguration, sql, disposeCommand, singleResult)
        {
            for (var i = 0; i < parameters.Count; i++)
            {
                _command.Parameters.Add(parameters[i]);
            }
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> TrackChanges(bool trackChanges = true)
        {
            _trackChanges = trackChanges;

            return this;
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> Log(bool shouldLog = true)
        {
            _shouldForceLog = shouldLog;

            return this;
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> LogTo(LoggerCallback logger)
        {
            _loggers.Add(logger);

            return this;
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> LogTo(params LoggerCallback[] loggers)
        {
            _loggers.AddRange(loggers);

            return this;
        }

        public IBaseQueryRelationBuilder<TEntity, TEntity, TReturn> AddFormatter()
        {
            _queryGenerationOptions |= QueryGenerationOptions.GenerateJoins;

            return this;
        }

        public IQueryCommand<TEntity, TReturn> Build()
        {
            if (_interpolatedSqlExpression is not null)
            {
                BuildFromExpression();
            }
            else if (_interpolatedSqlParameters is null)
            {
                if ((_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0 &&
                    _relationBuilderValues is not null)
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
                BuildFromInterpolatedSql();
            }

            var shouldLog = _shouldForceLog ?? _database.DefaultLoggingBehavior == LoggingBehavior.Always || _loggers.Count != 0;

            return new VenflowQueryCommand<TEntity, TReturn>(_database, _entityConfiguration, _command, _relationBuilderValues, _trackChanges, _disposeCommand, _singleResult && _relationBuilderValues is null, _loggers, shouldLog);
        }

        private void BuildFromExpression()
        {
            var cacheKey = _interpolatedSqlExpression!.Body.ToString();

            var expressionOptions = SqlExpressionOptions.None;
            Delegate argumentsFunc = null!;
            string sql = null!;
            Type? parameterType = null;

            if (!_entityConfiguration.MaterializerFactory.InterpolatedSqlMaterializerCache.TryGetValue(cacheKey, out var sqlExpression))
            {
                lock (_entityConfiguration.MaterializerFactory.InterpolatedSqlMaterializerCache)
                {
                    if (!_entityConfiguration.MaterializerFactory.InterpolatedSqlMaterializerCache.TryGetValue(cacheKey, out sqlExpression))
                    {
                        if (_interpolatedSqlExpression.Body is not MethodCallExpression method)
                            throw new InvalidOperationException("The body of this Expression has to be an interpolated string.");

                        var parameters = _interpolatedSqlExpression.Parameters;

                        var expressionArguments = (method.Arguments[1] as NewArrayExpression)!.Expressions;

                        var staticArguments = new List<(int, string)>();
                        var instanceArguments = new List<Expression>();

                        for (var expressionArgumentIndex = 0; expressionArgumentIndex < expressionArguments.Count; expressionArgumentIndex++)
                        {
                            var argument = expressionArguments[expressionArgumentIndex];

                            bool isConverted;

                            if (argument.NodeType == ExpressionType.Convert)
                            {
                                argument = (argument as UnaryExpression)!.Operand;

                                isConverted = true;
                            }
                            else
                            {
                                isConverted = false;
                            }

                            MemberExpression? memberArgument = null;
                            ParameterExpression? parameterArgument = null;

                            if (argument.NodeType == ExpressionType.MemberAccess)
                            {
                                memberArgument = (argument as MemberExpression)!;

                                parameterArgument = memberArgument.Expression as ParameterExpression;

                                if (parameterArgument is null)
                                    memberArgument = null;
                            }
                            else if (argument.NodeType == ExpressionType.Parameter)
                            {
                                parameterArgument = argument as ParameterExpression;
                            }

                            if (parameterArgument is null)
                            {
                                instanceArguments.Add(isConverted ? Expression.Convert(argument, typeof(object)) : argument);

                                continue;
                            }

                            string? name = null;

                            for (var expressionParameterIndex = 0; expressionParameterIndex < parameters.Count; expressionParameterIndex++)
                            {
                                var parameter = parameters[expressionParameterIndex];

                                if (parameterArgument.Name != parameter.Name)
                                    continue;

                                if (!_database.Entities.TryGetValue(parameter.Type.Name, out var entity))
                                    throw new InvalidOperationException($"The generic type parameter '{parameter.Type.Name}' is not a valid entity.");

                                if (memberArgument is not null)
                                {
                                    for (var columnIndex = 0; columnIndex < entity.GetColumnCount(); columnIndex++)
                                    {
                                        var column = entity.GetColumn(columnIndex);

                                        if (column.PropertyInfo.Name != memberArgument.Member.Name)
                                            continue;

                                        name = entity.TableName + "." + column.NormalizedColumnName;

                                        break;
                                    }
                                }
                                else
                                {
                                    name = entity.TableName;
                                }

                                break;
                            }

                            if (name is null)
                                throw new InvalidOperationException($"The property {memberArgument!.Member.Name} is not mapped as a column.");

                            staticArguments.Add((expressionArgumentIndex, name));
                        }

                        (sql, var dbTypes) = GetFinalizedSqlString(((method.Arguments[0] as ConstantExpression)!.Value as string)!, staticArguments);

                        (argumentsFunc, expressionOptions, parameterType) = InterpolatedSqlExpressionConverter.GetConvertedDelegate(instanceArguments, dbTypes);

                        _entityConfiguration.MaterializerFactory.InterpolatedSqlMaterializerCache.Add(cacheKey, new SqlExpression(sql, argumentsFunc, parameterType!, expressionOptions));
                    }
                }
            }

            if (sqlExpression is not null)
            {
                sql = sqlExpression.SQL;
                argumentsFunc = sqlExpression.Arguments;
                expressionOptions = sqlExpression.Options;
                parameterType = sqlExpression.ParameterType;
            }

            object[] arguments;

            if (expressionOptions == SqlExpressionOptions.None)
            {
                arguments = (argumentsFunc as Func<object[]>)!.Invoke();
            }
            else
            {
                arguments = (argumentsFunc as Func<object, object[]>)!.Invoke(InterpolatedSqlExpressionConverter.ExtractInstance(_interpolatedSqlExpression!, parameterType!)!);
            }

            var argumentsSpan = arguments.AsSpan();
            var sqlLength = sql.Length;
            var argumentedSql = new StringBuilder(sqlLength);
            var sqlSpan = sql.AsSpan();

            var argumentIndex = 0;
            var parameterIndex = 0;

            for (var spanIndex = 0; spanIndex < sqlLength; spanIndex++)
            {
                var spanChar = sqlSpan[spanIndex];

                if (spanChar == '{' &&
                    spanIndex + 2 < sqlLength)
                {
                    for (spanIndex++; spanIndex < sqlLength; spanIndex++)
                    {
                        spanChar = sqlSpan[spanIndex];

                        if (spanChar == '}')
                            break;

                        if (spanChar is < '0' or > '9')
                        {
                            throw new InvalidOperationException();
                        }
                    }

                    var argument = argumentsSpan[argumentIndex++];
                    NpgsqlDbType? dbType = default;

                    if (argument is Tuple<object, NpgsqlDbType> tuple)
                    {
                        argument = tuple.Item1;
                        dbType = tuple.Item2;
                    }

                    if (argument is IList list)
                    {
                        if (list.Count > 0)
                        {
                            var listType = default(Type);

                            for (var listIndex = 0; listIndex < list.Count; listIndex++)
                            {
                                var listItem = list[listIndex];

                                if (listType is null &&
                                    listItem is not null)
                                {
                                    listType = listItem.GetType();

                                    if (listType == typeof(object))
                                        throw new InvalidOperationException("The SQL string interpolation doesn't support object lists.");
                                }

                                var parameterName = "@p" + parameterIndex++.ToString();

                                argumentedSql.Append(parameterName)
                                             .Append(", ");

                                if (dbType is null)
                                {
                                    _command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, listType!, listItem));
                                }
                                else
                                {
                                    _command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, listItem, dbType.Value));
                                }
                            }

                            argumentedSql.Length -= 2;
                        }

                        parameterIndex--;
                    }
                    else
                    {
                        var parameterName = "@p" + parameterIndex++.ToString();

                        argumentedSql.Append(parameterName);

                        if (dbType is null)
                        {
                            _command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, argument));
                        }
                        else
                        {
                            _command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, argument, dbType.Value));
                        }
                    }
                }
                else
                {
                    argumentedSql.Append(spanChar);
                }
            }

            _command.CommandText = argumentedSql.ToString();
        }

        private (string Sql, List<(int Index, NpgsqlDbType DbType)> DbTypes) GetFinalizedSqlString(string sql, List<(int Index, string Name)> staticArguments)
        {
            var hasGeneratedJoins = (_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0 && _relationBuilderValues is not null;
            var sqlLength = sql.Length;
            var argumentedSql = new StringBuilder(sqlLength);
            var sqlSpan = sql.AsSpan();

            var dbTypes = new List<(int Index, NpgsqlDbType DbType)>();

            var argumentIndex = 0;
            var staticArgumentIndex = 1;
            var nextStaticArgument = staticArguments.Count == 0 ? (-1, null) : staticArguments[0];

            for (var spanIndex = 0; spanIndex < sqlLength; spanIndex++)
            {
                var spanChar = sqlSpan[spanIndex];

                if (spanChar == '{' &&
                    spanIndex + 2 < sqlLength)
                {
                    var dbTypeString = string.Empty;
                    var appendDbTypeString = false;

                    for (spanIndex++; spanIndex < sqlLength; spanIndex++)
                    {
                        spanChar = sqlSpan[spanIndex];

                        if (spanChar == '}')
                            break;

                        if (spanChar == ',')
                        {
                            appendDbTypeString = true;
                        }
                        else if (spanChar is < '0' or > '9')
                        {
                            throw new InvalidOperationException();
                        }
                        else if (appendDbTypeString)
                        {
                            dbTypeString += spanChar.ToString();
                        }
                    }

                    if (dbTypeString != string.Empty)
                    {
                        dbTypes.Add((argumentIndex - staticArgumentIndex + 1, (NpgsqlDbType)int.Parse(dbTypeString)));
                    }

                    if (argumentIndex == nextStaticArgument.Index)
                    {
                        argumentedSql.Append(nextStaticArgument.Name);

                        if (staticArguments.Count > staticArgumentIndex)
                            nextStaticArgument = staticArguments[staticArgumentIndex];

                        staticArgumentIndex++;
                    }
                    else
                    {
                        argumentedSql.Append("{0}");
                    }

                    argumentIndex++;
                }
                else if (hasGeneratedJoins &&
                         spanChar == '>' &&
                         spanIndex + 1 < sqlLength &&
                         sqlSpan[spanIndex + 1] == '<')
                {
                    hasGeneratedJoins = false;

                    AppendJoins(argumentedSql);

                    spanIndex++;
                }
                else
                {
                    argumentedSql.Append(spanChar);
                }
            }

            return (argumentedSql.ToString(), dbTypes);
        }

        private void BuildFromInterpolatedSql()
        {
            var hasGeneratedJoins = (_queryGenerationOptions & QueryGenerationOptions.GenerateJoins) != 0 && _relationBuilderValues is not null;
            var argumentsSpan = _interpolatedSqlParameters.AsSpan();
            var sqlLength = _rawSql.Length;
            var argumentedSql = new StringBuilder(sqlLength);
            var sqlSpan = _rawSql.AsSpan();

            var argumentIndex = 0;
            var parameterIndex = 0;

            for (var spanIndex = 0; spanIndex < sqlLength; spanIndex++)
            {
                var spanChar = sqlSpan[spanIndex];

                if (spanChar == '{' &&
                    spanIndex + 2 < sqlLength)
                {
                    for (spanIndex++; spanIndex < sqlLength; spanIndex++)
                    {
                        spanChar = sqlSpan[spanIndex];

                        if (spanChar == '}')
                            break;

                        if (spanChar is < '0' or > '9')
                        {
                            throw new InvalidOperationException();
                        }
                    }

                    var argument = argumentsSpan[argumentIndex++];

                    if (argument is IList list)
                    {
                        if (list.Count > 0)
                        {
                            var listType = default(Type);

                            for (var listIndex = 0; listIndex < list.Count; listIndex++)
                            {
                                var listItem = list[listIndex];

                                if (listType is null &&
                                    listItem is not null)
                                {
                                    listType = listItem.GetType();

                                    if (listType == typeof(object))
                                        throw new InvalidOperationException("The SQL string interpolation doesn't support object lists.");
                                }

                                var parameterName = "@p" + parameterIndex++.ToString();

                                argumentedSql.Append(parameterName)
                                             .Append(", ");

                                _command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, listType!, listItem));
                            }

                            argumentedSql.Length -= 2;
                        }

                        parameterIndex--;
                    }
                    else
                    {
                        var parameterName = "@p" + parameterIndex++.ToString();

                        argumentedSql.Append(parameterName);

                        _command.Parameters.Add(ParameterTypeHandler.HandleParameter(parameterName, argument));
                    }
                }
                else if (hasGeneratedJoins &&
                         spanChar == '>' &&
                         spanIndex + 1 < sqlLength &&
                         sqlSpan[spanIndex + 1] == '<')
                {
                    hasGeneratedJoins = false;

                    AppendJoins(argumentedSql);

                    spanIndex++;
                }
                else
                {
                    argumentedSql.Append(spanChar);
                }
            }

            _command.CommandText = argumentedSql.ToString();
        }

        private void AppendJoins(StringBuilder sb)
        {
            var relationsSpan = _relationBuilderValues!.FlattenedPath.AsSpan();

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

                sb.Append(relation.RightEntity.TableName)
                  .Append(" ON ");

                if (relation.ForeignKeyLocation == ForeignKeyLocation.Left)
                {
                    sb.Append(relation.LeftEntity.TableName)
                      .Append(".")
                      .Append(relation.ForeignKeyColumn.NormalizedColumnName)
                      .Append(" = ")
                      .Append(relation.RightEntity.TableName)
                      .Append(".")
                      .Append(relation.RightEntity.GetPrimaryColumn()!.NormalizedColumnName);
                }
                else
                {
                    sb.Append(relation.RightEntity.TableName)
                      .Append(".")
                      .Append(relation.ForeignKeyColumn.NormalizedColumnName)
                      .Append(" = ")
                      .Append(relation.LeftEntity.TableName)
                      .Append(".")
                      .Append(relation.LeftEntity.GetPrimaryColumn()!.NormalizedColumnName);
                }
            }
        }

        public Task<TReturn?> QueryAsync(CancellationToken cancellationToken = default)
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
        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.LeftJoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.LeftJoinWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.LeftJoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.LeftJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.RightJoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.RightJoinWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.RightJoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.RightJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.FullJoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.FullJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.FullJoinWith<TToEntity>(Expression<Func<TEntity, IList<TToEntity>>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.FullJoin);

        IQueryRelationBuilder<TToEntity, TEntity, TReturn> IBaseQueryRelationBuilder<TEntity, TEntity, TReturn>.FullJoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector)
                => JoinWith(propertySelector, JoinBehaviour.FullJoin);
    }
}
