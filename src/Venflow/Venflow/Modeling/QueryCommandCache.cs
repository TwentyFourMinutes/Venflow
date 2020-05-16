using Npgsql;
using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Venflow.Modeling
{
    internal class QueryCommandCache<TEntity> where TEntity : class
    {
        private readonly EntityColumnCollection<TEntity> _columns;
        private readonly Dictionary<ulong, Func<NpgsqlDataReader, TEntity>> _factories;
        private readonly Dictionary<ulong, Func<NpgsqlDataReader, TEntity>> _changeTrackingFactories;
        private readonly NewExpression _entityTypeConstructorExpression;

        private readonly Type? _proxyType;
        private readonly ParameterExpression? _changeTrackerVariable;
        private readonly BinaryExpression? _changeTracker;
        private readonly NewExpression? _proxyEntityTypeConstructorExpression;
        private readonly ParameterExpression? _proxyEntityVariable;
        private readonly BinaryExpression? _proxyChangeTrackerActivator;
        private readonly UnaryExpression? _proxyTypeCast;

        internal QueryCommandCache(Type entityType, Type? proxyType, EntityColumnCollection<TEntity> columns)
        {
            _entityTypeConstructorExpression = Expression.New(entityType);

            if (proxyType is { })
            {
                _proxyType = proxyType;

                var changeTrackerType = typeof(ChangeTracker<TEntity>);

                _changeTrackerVariable = Expression.Variable(changeTrackerType, "changeTracker");
                _changeTracker = Expression.Assign(_changeTrackerVariable, Expression.New(changeTrackerType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { TypeCache.Int32, TypeCache.Boolean }, null)!, Expression.Constant(columns.Count), ExpressionCache.FalseConstant));
                _proxyEntityTypeConstructorExpression = Expression.New(proxyType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { changeTrackerType }, null)!, _changeTracker);
                _proxyEntityVariable = Expression.Variable(proxyType, "proxyInstance");
                _proxyChangeTrackerActivator = Expression.Assign(Expression.Property(_changeTrackerVariable, "TrackChanges"), ExpressionCache.TrueConstant);
                _proxyTypeCast = Expression.Convert(_proxyEntityVariable, entityType);
            }

            _columns = columns;
            _factories = new Dictionary<ulong, Func<NpgsqlDataReader, TEntity>>();
            _changeTrackingFactories = new Dictionary<ulong, Func<NpgsqlDataReader, TEntity>>();
        }

        internal Func<NpgsqlDataReader, TEntity> GetOrCreateFactory(ReadOnlyCollection<NpgsqlDbColumn> columnSchema, bool changeTracking)
        {
            var columns = new EntityColumn<TEntity>[columnSchema.Count];
            var columnOrdinals = new int[columnSchema.Count];
            var columnFlags = 0uL;

            for (int i = 0; i < columnSchema.Count; i++)
            {
                var columnScheme = columnSchema[i];

                var column = _columns[columnScheme.ColumnName];

                columnFlags |= column.FlagValue;

                columnOrdinals[i] = columnScheme.ColumnOrdinal.Value;

                columns[i] = column;
            }

            var factories = changeTracking ? _changeTrackingFactories : _factories;

            if (factories.TryGetValue(columnFlags, out var factory))
            {
                return factory;
            }
            else
            {
                factory = CreateFactory(columns, columnOrdinals, changeTracking);

                factories.Add(columnFlags, factory);

                return factory;
            }
        }

        private Func<NpgsqlDataReader, TEntity> CreateFactory(EntityColumn<TEntity>[] columns, int[] columnOrdinals, bool changeTracking)
        {
            var propertyBindings = new MemberBinding[columns.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                var column = columns[i];

                propertyBindings[i] = GenerateMemberBinding(column, columnOrdinals[i]);
            }

            if (changeTracking)
            {
                var block = Expression.Block(_proxyType!, new[] { _proxyEntityVariable!, _changeTrackerVariable! },
                                             _changeTracker!,
                                             Expression.Assign(_proxyEntityVariable!, Expression.MemberInit(_proxyEntityTypeConstructorExpression!, propertyBindings)),
                                             _proxyChangeTrackerActivator!,
                                             _proxyTypeCast!);

                return Expression.Lambda<Func<NpgsqlDataReader, TEntity>>(block, ExpressionCache.NpgsqlDataReaderParameter).Compile();
            }
            else
            {
                return Expression.Lambda<Func<NpgsqlDataReader, TEntity>>(Expression.MemberInit(_entityTypeConstructorExpression, propertyBindings), ExpressionCache.NpgsqlDataReaderParameter).Compile();
            }
        }

        private MemberBinding GenerateMemberBinding(EntityColumn<TEntity> column, int columnOrdinal)
        {
            return Expression.Bind(column.PropertyInfo.GetSetMethod(), Expression.Call(ExpressionCache.NpgsqlDataReaderParameter, column.DbValueRetriever, Expression.Constant(columnOrdinal)));
        }
    }
}
