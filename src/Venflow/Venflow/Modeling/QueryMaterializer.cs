using Npgsql;
using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Venflow.Modeling
{
    internal class QueryMaterializer<TEntity> where TEntity : class
    {
        private DbConfiguration _dbConfiguration;
        private ReadOnlyCollection<NpgsqlDbColumn> _columnSchema;
        private uint _lastTableOID;

        private Func<NpgsqlDataReader, List<TEntity>>? _materializer;

        private List<ParameterExpression> _variables;
        private ConditionalExpression _ifExpression;
        private List<MemberAssignment> _memberAssignment;
        private ParameterExpression _primaryKeyVariable;
        private BinaryExpression _primaryKeyAssignment;
        private Entity? _currentEntity;

        internal QueryMaterializer(DbConfiguration dbConfiguration, ReadOnlyCollection<NpgsqlDbColumn> columnSchema)
        {
            _dbConfiguration = dbConfiguration;
            _columnSchema = columnSchema;
            _variables = new List<ParameterExpression>();
            _memberAssignment = new List<MemberAssignment>();
        }

        internal Func<NpgsqlDataReader, List<TEntity>> GetOrBuildMaterializer()
        {
            if (_materializer is { })
                return _materializer;

            Build();

            _dbConfiguration = null!;
            _columnSchema = null!;

            return _materializer!;
        }

        private void Build()
        {
            ComposeEntities();
        }

        private void ComposeEntities()
        {
            // Looping over all columns in a reverse order, since while constructing the expression, we need the inner 'ifs' first.
            for (int i = _columnSchema.Count; i > 0; i--)
            {
                var column = _columnSchema[i];

                // If we hit a primary key, then we want to construct the entity expression.
                if (column.IsKey == true && _lastTableOID != column.TableOID)
                {
                    BuildEntity(_currentEntity!.GetColumn(column.ColumnName), column.ColumnOrdinal!.Value);
                }
                else
                {
                    // If we hit a column, which is in a different table, get and set the table as the current one.
                    if (_lastTableOID != column.TableOID)
                    {
                        // Get the entity for a table, if it exists.
                        if (_dbConfiguration.TableEntities.TryGetValue(column.BaseTableName, out var entity))
                        {
                            _lastTableOID = column.TableOID;
                            _currentEntity = entity;
                        }
                        else
                        {
                            throw new InvalidOperationException($"The table '{column.BaseTableName}' doesn't got any matching entities in the current configuration.");
                        }
                    }

                    AddColumn(_currentEntity!.GetColumn(column.ColumnName), column.ColumnOrdinal!.Value);
                }
            }
        }

        private void BuildEntity(EntityColumn primaryColumn, int primaryColumnOrdinal)
        {
            var dictionaryType = TypeCache.GenericDictionary.MakeGenericType(new[] { TypeCache.String, _currentEntity.EntityType });

            var dictionary = Expression.Variable(dictionaryType);
            var currentPrimaryKeyVariable = Expression.Variable(primaryColumn.PropertyInfo.PropertyType);

            _variables.Add(dictionary);

            _primaryKeyAssignment = Expression.Assign(currentPrimaryKeyVariable, Expression.Call(ExpressionCache.NpgsqlDataReaderParameter, primaryColumn.DbValueRetriever, Expression.Constant(primaryColumnOrdinal)));

            if (primaryColumnOrdinal == 0)
            {

            }
            else
            {

            }

            AddColumn(primaryColumn, currentPrimaryKeyVariable);

            var dictionaryCheck = Expression.Call(dictionary, "TryGetValue", new[] { _currentEntity.EntityType }, currentPrimaryKeyVariable);

            //Expression.Call

            //var memberInit = Expression.MemberInit(Expression.New(_currentEntity!.EntityType), _memberAssignment);

            //if (_ifExpression is null)
            //{
            //    _ifExpression = Expression.IfThen(Expression.IsFalse(dictionaryCheck), Expression.Block(_memberAssignment));
            //}
            //else
            //{
            //    var ifExpressionContent = Expression.Block(new[] { _primaryKeyVariable }, _primaryKeyAssignment, _ifExpression);

            //    _ifExpression = Expression.IfThenElse(dictionaryCheck, ifExpressionContent, Expression.Block(_memberAssignment));
            //}

            //_primaryKeyVariable = currentPrimaryKeyVariable;
            //_memberAssignment.Clear();
        }

        private void AddColumn(EntityColumn column, int columnOrdinal)
        {
            AddColumn(column, Expression.Call(ExpressionCache.NpgsqlDataReaderParameter, column.DbValueRetriever, Expression.Constant(columnOrdinal)));
        }

        private void AddColumn(EntityColumn column, Expression assignExpression)
        {
            _memberAssignment.Add(Expression.Bind(column.PropertyInfo.GetSetMethod()!, assignExpression));
        }
    }
}