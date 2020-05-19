using Npgsql;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Venflow.Modeling;

namespace Venflow.Commands
{
    internal class VenflowCommandBuilder<TEntity> : IVenflowCommandBuilder<TEntity> where TEntity : class
    {
        internal bool IsSingle { get; private set; }
        internal bool TrackingChanges { get; private set; }
        internal bool GetComputedColumns { get; private set; }
        internal bool DisposeCommand { get; }

        private readonly Entity<TEntity> _entityConfiguration;
        private readonly StringBuilder _commandString;
        private readonly NpgsqlCommand _command;

        internal VenflowCommandBuilder(Entity<TEntity> entityConfiguration, bool disposeCommand = false)
        {
            _entityConfiguration = entityConfiguration;
            DisposeCommand = disposeCommand;
            _commandString = new StringBuilder();
            _command = new NpgsqlCommand();
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

        IQueryCommand<TEntity> IQueryCommandBuilder<TEntity>.Single()
        {
            IsSingle = true;

            return BaseQuery(1);
        }

        IQueryCommand<TEntity> IQueryCommandBuilder<TEntity>.Single(string sql, params NpgsqlParameter[] parameters)
        {
            IsSingle = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                _command.Parameters.Add(parameters[i]);
            }

            _commandString.Append(sql);

            return BuildCommand();
        }

        IQueryCommand<TEntity> IQueryCommandBuilder<TEntity>.Batch()
             => BaseQuery(0);

        IQueryCommand<TEntity> IQueryCommandBuilder<TEntity>.Batch(ulong count)
             => BaseQuery(count);

        IQueryCommand<TEntity> IQueryCommandBuilder<TEntity>.Batch(string sql, params NpgsqlParameter[] parameters)
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
            _commandString.Append("SELECT ");

            _commandString.Append(_entityConfiguration.ColumnListString);

            _commandString.Append(" FROM ");

            _commandString.Append(_entityConfiguration.TableName);

            if (count > 0)
            {
                _commandString.Append(" LIMIT ");
                _commandString.Append(count);
                _commandString.Append(';');
            }

            return BuildCommand();
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

            return new VenflowCommand<TEntity>(_command, _entityConfiguration) { GetComputedColumns = GetComputedColumns, IsSingle = IsSingle, TrackingChanges = TrackingChanges, DisposeCommand = DisposeCommand };
        }
    }
}