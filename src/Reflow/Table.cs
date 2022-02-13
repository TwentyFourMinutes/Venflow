using Reflow.Operations;

namespace Reflow
{
    internal static class InstanceStore<T> where T : class, new()
    {
        internal static T Instance = new();
    }

    public class Table<TEntity> where TEntity : class, new()
    {
        private readonly IDatabase _database;

        public Table(object database)
        {
            _database = (IDatabase)database;
        }

        #region Query

        public QueryBuilder<TEntity> Query(Func<SqlInterpolationHandler> sql)
        {
            Operations.Query.Handle(_database, sql, static x => x.Invoke());

            return default;
        }

        public QueryBuilder<TEntity> Query(Func<TEntity, SqlInterpolationHandler> sql)
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x => x.Invoke(InstanceStore<TEntity>.Instance)
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1>(Func<TEntity, T1, SqlInterpolationHandler> sql)
            where T1 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x => x.Invoke(InstanceStore<TEntity>.Instance, InstanceStore<T1>.Instance)
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1, T2>(
            Func<TEntity, T1, T2, SqlInterpolationHandler> sql
        )
            where T1 : class, new()
            where T2 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x =>
                    x.Invoke(
                        InstanceStore<TEntity>.Instance,
                        InstanceStore<T1>.Instance,
                        InstanceStore<T2>.Instance
                    )
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1, T2, T3>(
            Func<TEntity, T1, T2, T3, SqlInterpolationHandler> sql
        )
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x =>
                    x.Invoke(
                        InstanceStore<TEntity>.Instance,
                        InstanceStore<T1>.Instance,
                        InstanceStore<T2>.Instance,
                        InstanceStore<T3>.Instance
                    )
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1, T2, T3, T4>(
            Func<TEntity, T1, T2, T3, T4, SqlInterpolationHandler> sql
        )
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x =>
                    x.Invoke(
                        InstanceStore<TEntity>.Instance,
                        InstanceStore<T1>.Instance,
                        InstanceStore<T2>.Instance,
                        InstanceStore<T3>.Instance,
                        InstanceStore<T4>.Instance
                    )
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1, T2, T3, T4, T5>(
            Func<TEntity, T1, T2, T3, T4, T5, SqlInterpolationHandler> sql
        )
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
            where T5 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x =>
                    x.Invoke(
                        InstanceStore<TEntity>.Instance,
                        InstanceStore<T1>.Instance,
                        InstanceStore<T2>.Instance,
                        InstanceStore<T3>.Instance,
                        InstanceStore<T4>.Instance,
                        InstanceStore<T5>.Instance
                    )
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1, T2, T3, T4, T5, T6>(
            Func<TEntity, T1, T2, T3, T4, T5, T6, SqlInterpolationHandler> sql
        )
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
            where T5 : class, new()
            where T6 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x =>
                    x.Invoke(
                        InstanceStore<TEntity>.Instance,
                        InstanceStore<T1>.Instance,
                        InstanceStore<T2>.Instance,
                        InstanceStore<T3>.Instance,
                        InstanceStore<T4>.Instance,
                        InstanceStore<T5>.Instance,
                        InstanceStore<T6>.Instance
                    )
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1, T2, T3, T4, T5, T6, T7>(
            Func<TEntity, T1, T2, T3, T4, T5, T6, T7, SqlInterpolationHandler> sql
        )
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
            where T5 : class, new()
            where T6 : class, new()
            where T7 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x =>
                    x.Invoke(
                        InstanceStore<TEntity>.Instance,
                        InstanceStore<T1>.Instance,
                        InstanceStore<T2>.Instance,
                        InstanceStore<T3>.Instance,
                        InstanceStore<T4>.Instance,
                        InstanceStore<T5>.Instance,
                        InstanceStore<T6>.Instance,
                        InstanceStore<T7>.Instance
                    )
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<TEntity, T1, T2, T3, T4, T5, T6, T7, T8, SqlInterpolationHandler> sql
        )
            where T1 : class, new()
            where T2 : class, new()
            where T3 : class, new()
            where T4 : class, new()
            where T5 : class, new()
            where T6 : class, new()
            where T7 : class, new()
            where T8 : class, new()
        {
            Operations.Query.Handle(
                _database,
                sql,
                static x =>
                    x.Invoke(
                        InstanceStore<TEntity>.Instance,
                        InstanceStore<T1>.Instance,
                        InstanceStore<T2>.Instance,
                        InstanceStore<T3>.Instance,
                        InstanceStore<T4>.Instance,
                        InstanceStore<T5>.Instance,
                        InstanceStore<T6>.Instance,
                        InstanceStore<T7>.Instance,
                        InstanceStore<T8>.Instance
                    )
            );

            return default;
        }

        public QueryBuilder<TEntity> QueryRaw(Func<string> sql)
        {
            Operations.Query.HandleRaw(_database, sql);

            return default;
        }

        #endregion

        #region Insert

        public Task InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            return Insert.InsertAsync(_database, entity, cancellationToken);
        }

        public Task InsertAsync(
            IList<TEntity> entities,
            CancellationToken cancellationToken = default
        )
        {
            return Insert.InsertAsync(_database, entities, cancellationToken);
        }

        #endregion
    }
}
