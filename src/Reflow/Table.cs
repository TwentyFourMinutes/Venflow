using Reflow.Commands;

namespace Reflow
{
    public class Table<T> where T : class, new()
    {
        private readonly IDatabase _database;

        public Table(object database)
        {
            _database = (IDatabase)database;
        }

        public QueryBuilder<T> Query(Func<SqlInterpolationHandler> sql)
        {
            Commands.Query.Handle(_database, sql);

            return default;
        }
    }
}
