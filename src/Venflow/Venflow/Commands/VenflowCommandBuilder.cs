using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Venflow.Enums;
using Venflow.Modeling;
using Venflow.Models;

namespace Venflow.Commands
{
    internal class VenflowCommandBuilder<TEntity> : IVenflowCommandBuilder<TEntity> where TEntity : class
    {
        internal JoinBuilderValues? JoinValues { get; set; }

        internal bool IsSingle { get; private set; }
        internal bool TrackingChanges { get; private set; }
        internal bool GetComputedColumns { get; private set; }

        internal bool DisposeCommand { get; }

        private readonly Entity<TEntity> _entityConfiguration;
        private readonly StringBuilder _commandString;
        private readonly NpgsqlCommand _command;

        internal VenflowCommandBuilder(Entity<TEntity> entityConfiguration, bool disposeCommand = false, NpgsqlCommand? command = null)
        {
            _entityConfiguration = entityConfiguration;
            DisposeCommand = disposeCommand;
            _commandString = new StringBuilder();
            _command = command ?? new NpgsqlCommand();
        }

        #region Query

        public IQueryCommandBuilder<TEntity> Query()
        {
            return this;
        }

        IQueryCommandBuilder<TEntity> IQueryCommandBuilder<TEntity>.TrackChanges(bool trackChanges)
        {
            TrackingChanges = trackChanges;

            return this;
        }

        public JoinBuilder<TEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            var builder = new JoinBuilder<TEntity, TToEntity>(_entityConfiguration, this);

            return builder.JoinWith(propertySelector, joinBehaviour);
        }

        public JoinBuilder<TEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            var builder = new JoinBuilder<TEntity, TToEntity>(_entityConfiguration, this);

            return builder.JoinWith(propertySelector, joinBehaviour);
        }


        public IQueryCommand<TEntity> Single()
        {
            IsSingle = true;

            return BaseQuery(1);
        }

        public IQueryCommand<TEntity> Single(string sql, params NpgsqlParameter[] parameters)
        {
            IsSingle = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                _command.Parameters.Add(parameters[i]);
            }

            _commandString.Append(sql);

            return BuildCommand();
        }

        public IQueryCommand<TEntity> Batch()
             => BaseQuery(0);

        public IQueryCommand<TEntity> Batch(ulong count)
             => BaseQuery(count);

        public IQueryCommand<TEntity> Batch(string sql, params NpgsqlParameter[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                _command.Parameters.Add(parameters[i]);
            }

            _commandString.Append(sql);

            return BuildCommand();
        }

        private IQueryCommand<TEntity> BaseQuery(ulong count)
        {
            if (JoinValues is null)
            {
                AppendBaseQuery(_commandString, count);
                _commandString.Append(';');
            }
            else
            {
                BaseOneToManyQuery(count);
            }

            return BuildCommand();
        }

        private void BaseOneToManyQuery(ulong count)
        {
            _commandString.Append("SELECT ");

            _commandString.Append(_entityConfiguration.ExplicitColumnListString);

            var subQuery = new StringBuilder();

            subQuery.Append(" FROM (");

            AppendBaseQuery(subQuery, count);

            subQuery.Append(") AS ");

            subQuery.Append(_entityConfiguration.RawTableName);

            JoinValues!.AppendColumnNamesAndJoins(_commandString, subQuery);

            _commandString.Append(subQuery);

            _commandString.Append(';');
        }

        private void AppendBaseQuery(StringBuilder sb, ulong count)
        {
            sb.Append("SELECT ");

            sb.Append(_entityConfiguration.ColumnListString);

            sb.Append(" FROM ");

            sb.Append(_entityConfiguration.TableName);

            if (count > 0)
            {
                sb.Append(" LIMIT ");
                sb.Append(count);
            }
        }

        #endregion

        #region Insert

        public IInsertCommandBuilder<TEntity> Insert()
        {
            return this;
        }

        IInsertCommandBuilder<TEntity> IInsertCommandBuilder<TEntity>.ReturnComputedColumns(bool returnComputedColumns)
        {
            if (returnComputedColumns)
            {
                GetComputedColumns = _entityConfiguration.PrimaryColumn.IsServerSideGenerated;
            }

            return this;
        }

        IInsertCommand<TEntity> IInsertCommandBuilder<TEntity>.Single(TEntity entity)
        {
            IsSingle = true;

            _commandString.Append("INSERT INTO ");
            _commandString.Append(_entityConfiguration.TableName);
            _commandString.Append(" (");
            _commandString.Append(_entityConfiguration.NonPrimaryColumnListString);
            _commandString.AppendLine(")");

            _commandString.Append("VALUES (");

            _entityConfiguration.InsertWriter(entity, _commandString, "0", _command.Parameters);

            _commandString.Append(')');

            if (GetComputedColumns)
            {
                _commandString.Append(" RETURNING \"");

                _commandString.Append(_entityConfiguration.PrimaryColumn.ColumnName);

                _commandString.Append('"');
            }

            return BuildCommand();
        }

        IInsertCommand<TEntity>? IInsertCommandBuilder<TEntity>.Batch(IEnumerable<TEntity> entities)
        {
            _commandString.Append("INSERT INTO ");
            _commandString.Append(_entityConfiguration.TableName);
            _commandString.Append(" (");
            _commandString.Append(_entityConfiguration.NonPrimaryColumnListString);
            _commandString.AppendLine(")");

            _commandString.Append("VALUES ");

            var index = 0;

            var insertWriter = _entityConfiguration.InsertWriter;

            if (entities is IList<TEntity> entitiesList)
            {
                if (entitiesList.Count == 0)
                {
                    return null;
                }

                while (true)
                {
                    _commandString.Append("(");

                    insertWriter(entitiesList[index], _commandString, index++.ToString(), _command.Parameters);

                    if (index < entitiesList.Count)
                    {
                        _commandString.Append("), ");
                    }
                    else
                    {
                        _commandString.Append(")");

                        break;
                    }
                }
            }
            else
            {
                foreach (var entity in entities)
                {
                    _commandString.Append("(");

                    insertWriter(entity, _commandString, index++.ToString(), _command.Parameters);

                    _commandString.Append("), ");
                }

                if (index == 0)
                    return null;

                _commandString.Remove(_commandString.Length - 3, 3);
            }

            if (GetComputedColumns)
            {
                _commandString.Append(" RETURNING \"");

                _commandString.Append(_entityConfiguration.PrimaryColumn.ColumnName);

                _commandString.Append('"');
            }

            return BuildCommand();
        }

        #endregion

        #region Delete

        public IDeleteCommandBuilder<TEntity> Delete()
        {
            return this;
        }

        IDeleteCommand<TEntity> IDeleteCommandBuilder<TEntity>.Single(TEntity entity)
        {
            IsSingle = true;

            _commandString.Append("DELETE FROM ");
            _commandString.AppendLine(_entityConfiguration.TableName);
            _commandString.Append(" WHERE \"");
            _commandString.Append(_entityConfiguration.PrimaryColumn.ColumnName);
            _commandString.Append("\" = ");

            var primaryParameter = _entityConfiguration.PrimaryColumn.ValueRetriever(entity, "0");

            _command.Parameters.Add(primaryParameter);

            _commandString.Append(primaryParameter.ParameterName);
            _commandString.Append(';');

            return BuildCommand();
        }

        IDeleteCommand<TEntity> IDeleteCommandBuilder<TEntity>.Batch(IEnumerable<TEntity> entities)
        {
            _commandString.Append("DELETE FROM ");
            _commandString.AppendLine(_entityConfiguration.TableName);
            _commandString.Append(" WHERE \"");
            _commandString.Append(_entityConfiguration.PrimaryColumn.ColumnName);
            _commandString.Append("\" IN (");

            var valueRetriever = _entityConfiguration.PrimaryColumn.ValueRetriever;

            if (entities is IList<TEntity> list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var parameter = valueRetriever.Invoke(list[i], i.ToString());

                    _commandString.Append(parameter.ParameterName);
                    _commandString.Append(", ");

                    _command.Parameters.Add(parameter);
                }
            }
            else
            {
                var index = 0;

                foreach (var entity in entities)
                {
                    var parameter = valueRetriever.Invoke(entity, index++.ToString());

                    _commandString.Append(parameter.ParameterName);
                    _commandString.Append(", ");

                    _command.Parameters.Add(parameter);
                }
            }

            _commandString.Remove(_commandString.Length - 2, 2);
            _commandString.Append(");");

            return BuildCommand();
        }

        #endregion

        #region Update

        public IUpdateCommandBuilder<TEntity> Update()
        {
            return this;
        }

        IUpdateCommand<TEntity>? IUpdateCommandBuilder<TEntity>.Single(TEntity entity)
        {
            IsSingle = true;

            BaseUpdate(entity, 0);

            if (_command.Parameters.Count == 0)
                return null;

            return BuildCommand();
        }

        IUpdateCommand<TEntity>? IUpdateCommandBuilder<TEntity>.Batch(IEnumerable<TEntity> entities)
        {
            if (entities is IList<TEntity> entitiesList)
            {
                for (int i = 0; i < entitiesList.Count; i++)
                {
                    BaseUpdate(entitiesList[i], i);
                }
            }
            else
            {
                var index = 0;

                foreach (var entity in entities)
                {
                    BaseUpdate(entity, index++);
                }
            }

            if (_command.Parameters.Count == 0)
                return null;

            return BuildCommand();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BaseUpdate(TEntity entity, int index)
        {
            if (!(entity is IEntityProxy<TEntity> proxy))
            {
                throw new InvalidOperationException("The provided entity is currently not being change tracked.");
            }
            else if (!proxy.ChangeTracker.IsDirty)
            {
                return;
            }

            _commandString.Append("UPDATE ");
            _commandString.Append(_entityConfiguration.TableName);
            _commandString.Append(" SET ");

            var columns = proxy.ChangeTracker.GetColumns()!;

            var entityColumns = _entityConfiguration.Columns;

            for (int i = 0; i < columns.Length; i++)
            {
                var columnIndex = columns[i];

                if (columnIndex == 0)
                    continue;

                var column = entityColumns[columnIndex];

                _commandString.Append('"');
                _commandString.Append(column.ColumnName);
                _commandString.Append("\" = ");

                var parameter = column.ValueRetriever(entity, index.ToString());

                _commandString.Append(parameter.ParameterName);

                _command.Parameters.Add(parameter);

                _commandString.Append(", ");
            }

            _commandString.Remove(_commandString.Length - 2, 2);

            _commandString.Append(" WHERE \"");

            _commandString.Append(_entityConfiguration.PrimaryColumn.ColumnName);

            _commandString.Append("\" = ");

            var primaryParameter = _entityConfiguration.PrimaryColumn.ValueRetriever(entity, "Return" + index.ToString());

            _command.Parameters.Add(primaryParameter);

            _commandString.Append(primaryParameter.ParameterName);

            _commandString.Append(';');
        }

        #endregion

        internal VenflowCommand<TEntity> BuildCommand()
        {
            _command.CommandText = _commandString.ToString();

            return new VenflowCommand<TEntity>(_command, _entityConfiguration) { GetComputedColumns = GetComputedColumns, IsSingle = IsSingle, TrackingChanges = TrackingChanges, DisposeCommand = DisposeCommand, Relations = null };
        }
    }

    internal class JoinBuilderValues
    {
        internal Entity Root { get; }
        internal List<JoinPath> FullPath { get; }

        private JoinPath _currentPath;

        internal JoinBuilderValues(Entity root)
        {
            FullPath = new List<JoinPath>();
            Root = root;
        }

        internal void AddToPath(JoinOptions joinOptions, bool newFullPath)
        {
            if (newFullPath)
            {
                for (int i = 0; i < FullPath.Count; i++)
                {
                    var path = FullPath[i];

                    if (object.ReferenceEquals(path.JoinOptions.JoinWith, joinOptions.JoinWith))
                    {
                        _currentPath = path;

                        return;
                    }

                    var match = path.GetPath(joinOptions.JoinWith);

                    if (match is { })
                    {
                        _currentPath = match;

                        return;
                    }
                }

                var newPath = new JoinPath(joinOptions, new StringBuilder());

                FullPath.Add(newPath);

                _currentPath = newPath;

                return;
            }
            else
            {
                var match = _currentPath.GetPath(joinOptions.JoinWith);

                if (match is { })
                {
                    _currentPath = match;

                    _currentPath.TrailingJoinPath.Add(match);

                    return;
                }
            }

            if (_currentPath.TrailingJoinPath.Count == 0)
            {
                _currentPath = new JoinPath(joinOptions, _currentPath.SqlJoins);
            }
            else
            {
                _currentPath = new JoinPath(joinOptions, _currentPath.GetNewSqlJoinsFromBasePath(_currentPath));
            }
        }

        internal void AppendColumnNamesAndJoins(StringBuilder sqlColumns, StringBuilder sqlJoins)
        {
            for (int i = 0; i < FullPath.Count; i++)
            {
                FullPath[i].AppendColumnNamesAndJoins(sqlColumns, sqlJoins);
            }
        }
    }

    internal class JoinPath
    {
        internal JoinOptions JoinOptions { get; }

        internal List<JoinPath> TrailingJoinPath { get; }

        internal StringBuilder SqlJoins { get; }

        private readonly int _joinLength;

        internal JoinPath(JoinOptions joinOptions, StringBuilder sqlJoins)
        {
            JoinOptions = joinOptions;
            TrailingJoinPath = new List<JoinPath>();
            SqlJoins = sqlJoins;

            AppendJoin();

            _joinLength = SqlJoins.Length;
        }

        internal JoinPath? GetPath(ForeignEntity foreignEntity)
        {
            for (int i = 0; i < TrailingJoinPath.Count; i++)
            {
                var joingingEntity = TrailingJoinPath[i];

                if (object.ReferenceEquals(TrailingJoinPath[i].JoinOptions.JoinWith, foreignEntity))
                {
                    return joingingEntity;
                }
            }

            return null;
        }

        internal StringBuilder GetNewSqlJoinsFromBasePath(JoinPath fromPath)
            => new StringBuilder()
                .Append(fromPath.SqlJoins, 0, fromPath._joinLength);

        // TODO: Build this while generating
        internal void AppendColumnNamesAndJoins(StringBuilder sqlColumns, StringBuilder sqlJoins)
        {
            sqlColumns.Append(", ");
            sqlColumns.Append(JoinOptions.JoinWith.Entity.ExplicitColumnListString);

            if (TrailingJoinPath.Count == 0)
            {
                sqlJoins.AppendLine();
                sqlJoins.Append(SqlJoins);
            }

            for (int i = 0; i < TrailingJoinPath.Count; i++)
            {
                TrailingJoinPath[i].AppendColumnNamesAndJoins(sqlColumns, sqlJoins);
            }
        }

        private void AppendJoin()
        {
            if (SqlJoins.Length > 0)
                SqlJoins.AppendLine();

            switch (JoinOptions.JoinBehaviour)
            {
                case JoinBehaviour.InnerJoin:
                    SqlJoins.Append("INNER JOIN ");
                    break;
                case JoinBehaviour.LeftJoin:
                    SqlJoins.Append("LEFT JOIN ");
                    break;
                case JoinBehaviour.RightJoin:
                    SqlJoins.Append("RIGHT JOIN ");
                    break;
                case JoinBehaviour.FullJoin:
                    SqlJoins.Append("FULL JOIN ");
                    break;
                default:
                    throw new InvalidOperationException($"Invalid state '{JoinOptions.JoinBehaviour}' for the JoinBehaviour on entity {JoinOptions.JoinWith.Entity.EntityName}");
            }

            SqlJoins.Append(JoinOptions.JoinWith.Entity.TableName);
            SqlJoins.Append(" AS ");
            SqlJoins.Append(JoinOptions.JoinWith.Entity.RawTableName);
            SqlJoins.Append(" ON ");
            SqlJoins.Append(JoinOptions.JoinWith.Entity.RawTableName);
            SqlJoins.Append(".\"");
            SqlJoins.Append(JoinOptions.JoinWith.ForeignKey.ColumnName);
            SqlJoins.Append("\" = ");
            SqlJoins.Append(JoinOptions.JoinFrom.RawTableName);
            SqlJoins.Append(".\"");
            SqlJoins.Append(JoinOptions.JoinFrom.GetPrimaryColumn().ColumnName);
            SqlJoins.Append('"');
        }
    }

    public class JoinBuilder<TBaseEntity, TEntity> where TBaseEntity : class where TEntity : class
    {
        private readonly DualKeyCollection<string, ForeignEntity>? _relations;
        private readonly JoinBuilderValues _joinBuilderValues;
        private readonly VenflowCommandBuilder<TBaseEntity> _commandBuilder;
        private readonly Entity _lastEntity;

        internal JoinBuilder(Entity root, VenflowCommandBuilder<TBaseEntity> commandBuilder)
        {
            _joinBuilderValues = new JoinBuilderValues(root);

            _relations = root.Relations;
            _lastEntity = root;
            _commandBuilder = commandBuilder;
        }

        internal JoinBuilder(JoinOptions joinOptions, Entity lastEntity, JoinBuilderValues joinBuilderValues, VenflowCommandBuilder<TBaseEntity> commandBuilder, bool newFullPath)
        {
            _relations = joinOptions.JoinWith.Entity.Relations;

            joinBuilderValues.AddToPath(joinOptions, newFullPath);

            _joinBuilderValues = joinBuilderValues;
            _lastEntity = lastEntity;
            _commandBuilder = commandBuilder;
        }

        public JoinBuilder<TBaseEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TBaseEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            ThrowIfJoinIsInvalid(propertySelector);

            var foreignPropertyType = typeof(TEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TBaseEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.Entity, _joinBuilderValues, _commandBuilder, true);
        }

        public JoinBuilder<TBaseEntity, TToEntity> JoinWith<TToEntity>(Expression<Func<TBaseEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            ThrowIfJoinIsInvalid(propertySelector);

            var foreignPropertyType = typeof(TEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_joinBuilderValues.Root.Relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TBaseEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.Entity, _joinBuilderValues, _commandBuilder, true);
        }


        public JoinBuilder<TBaseEntity, TToEntity> ThenWith<TToEntity>(Expression<Func<TEntity, TToEntity>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            ThrowIfJoinIsInvalid(propertySelector);

            var foreignPropertyType = typeof(TToEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TBaseEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.Entity, _joinBuilderValues, _commandBuilder, false);
        }

        public JoinBuilder<TBaseEntity, TToEntity> ThenWith<TToEntity>(Expression<Func<TEntity, List<TToEntity>>> propertySelector, JoinBehaviour joinBehaviour = JoinBehaviour.InnerJoin) where TToEntity : class
        {
            ThrowIfJoinIsInvalid(propertySelector);

            var foreignPropertyType = typeof(TToEntity);

            var propertyName = foreignPropertyType.IsGenericType ? foreignPropertyType.GetGenericArguments()[0].Name : foreignPropertyType.Name;

            if (!_relations!.TryGetValue(propertyName, out var joiningEntity))
            {
                throw new TypeArgumentException($"The provided entity '{typeof(TToEntity).Name}' isn't in any relation with the entity '{typeof(TEntity).Name}'.");
            }

            return new JoinBuilder<TBaseEntity, TToEntity>(new JoinOptions(joiningEntity!, _lastEntity, joinBehaviour), joiningEntity!.Entity, _joinBuilderValues, _commandBuilder, false);
        }

        public IQueryCommand<TBaseEntity> Single()
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Single();
        }

        public IQueryCommand<TBaseEntity> Single(string sql, params NpgsqlParameter[] parameters)
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Single(sql, parameters);
        }

        public IQueryCommand<TBaseEntity> Batch()
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Batch();
        }

        public IQueryCommand<TBaseEntity> Batch(ulong count)
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Batch(count);
        }

        public IQueryCommand<TBaseEntity> Batch(string sql, params NpgsqlParameter[] parameters)
        {
            _commandBuilder.JoinValues = _joinBuilderValues;

            return _commandBuilder.Batch(sql, parameters);
        }

        private void ThrowIfJoinIsInvalid<TFromEntity, TToEntity>(Expression<Func<TFromEntity, TToEntity>> propertySelector) where TFromEntity : class where TToEntity : class
        {
            if (_relations is null)
            {
                throw new InvalidOperationException($"The current entity '{typeof(TFromEntity).Name}' doesn't have any relation with other entities.");
            }

            if (propertySelector is null)
            {
                throw new ArgumentNullException(nameof(propertySelector));
            }

            var body = propertySelector.Body as MemberExpression;

            if (body is null)
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property.", nameof(propertySelector));
            }

            var property = body.Member as PropertyInfo;

            if (property is null)
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property.", nameof(propertySelector));
            }

            if (!property.CanWrite || !property.SetMethod.IsPublic)
            {
                throw new ArgumentException($"The provided property doesn't contain a setter or it isn't public.", nameof(propertySelector));
            }

            var baseType = typeof(TFromEntity);

            if (baseType != property.ReflectedType &&
                !baseType.IsSubclassOf(property.ReflectedType!))
            {
                throw new ArgumentException($"The provided {nameof(propertySelector)} is not pointing to a property on the entity itself.", nameof(propertySelector));
            }
        }
    }
}