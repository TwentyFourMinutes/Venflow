namespace Reflow
{
    public class Table<T> where T : class, new()
    {
        private readonly IDatabase _database;

        public Table(IDatabase database)
        {
            _database = database;
        }
    }
}
