using Reflow.Commands;

namespace Reflow
{
    public class Table<T> where T : class, new()
    {
        private readonly IDatabase _database;

        public Table(IDatabase database)
        {
            _database = database;
        }

        public QueryBuilder<T> Query(Func<SqlInterpolationHandler> sql)
        {
            Commands.Query.Handle(sql);

            return default;
        }
    }
}
