using Reflow.Commands;

namespace Reflow
{
    internal static class InstanceStore<T> where T : class, new()
    {
        internal static T Instance { get; } = new();
    }

    public class Table<TEntity> where TEntity : class, new()
    {
        private readonly IDatabase _database;

        public Table(object database)
        {
            _database = (IDatabase)database;
        }

        public QueryBuilder<TEntity> Query(Func<SqlInterpolationHandler> sql)
        {
            Commands.Query.Handle(_database, sql, static x => x.Invoke());

            return default;
        }

        public QueryBuilder<TEntity> Query(Func<TEntity, SqlInterpolationHandler> sql)
        {
            Commands.Query.Handle(
                _database,
                sql,
                static x => x.Invoke(InstanceStore<TEntity>.Instance)
            );

            return default;
        }

        public QueryBuilder<TEntity> Query<T1>(Func<TEntity, T1, SqlInterpolationHandler> sql)
            where T1 : class, new()
        {
            Commands.Query.Handle(
                _database,
                sql,
                static x => x.Invoke(InstanceStore<TEntity>.Instance, InstanceStore<T1>.Instance)
            );

            return default;
        }

        public QueryBuilder<TEntity> QueryRaw(Func<string> sql)
        {
            Commands.Query.HandleRaw(_database, sql);

            return default;
        }
    }
}
