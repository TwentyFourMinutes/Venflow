using System.Data;
using System.Data.Common;
using System.Reflection;
using Npgsql;

namespace Reflow
{
    public class Table<TEntity> where TEntity : class
    {
        private readonly Database _database;

        public Table(Database database)
        {
            _database = database;
        }

        public QueryBuilder<TEntity> Query(Func<FormattableString> sql)
        {
            return QueryBuilder<TEntity>.GetBuilder(_database, sql);
        }
    }

    public class QueryBuilder<TEntity>
    {
        [ThreadStatic]
        private static readonly QueryBuilder<TEntity> _default = new QueryBuilder<TEntity>();

        private Database _database = null!;
        private Func<FormattableString> _sql = null!;

        private QueryBuilder()
        {
        }

        public static QueryBuilder<TEntity> GetBuilder(Database database, Func<FormattableString> sql)
        {
            var builder = _default;

            builder._database = database;
            builder._sql = sql;

            return builder;
        }

        public async Task<TEntity> SingleAsync()
        {
            await _database.EnsureConnectionEstablishedAsync(default);

            var formattableString = _sql.Invoke();

            var arguments = formattableString.GetArguments();

            Console.WriteLine(Database.LambdaLinks[_sql.Method]);

            return default;
        }
    }

    public class PostgreDatabase : Database
    {
        public Table<Person> People { get; }

        public PostgreDatabase() : base("")
        {
            People = new(this);
        }
    }

    public class Person
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class Database
    {
        internal static Dictionary<MethodInfo, string> LambdaLinks = new Dictionary<MethodInfo, string>();

        static Database()
        {
            //foreach (var item in LambdaLinker.Links)
            //{
            //    var methodInfos = item.ClassType.GetNestedType("<>c", BindingFlags.NonPublic)!.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).ToList();

            //    var methodInfo = methodInfos.FirstOrDefault(x => x.Name == item.FullLambdaName);

            //    LambdaLinks.Add(methodInfo, item.Content);
            //}

            //foreach (var item in LambdaLinker.ClosureLinks)
            //{
            //    var methodInfos = item.ClassType.GetNestedType(item.FullDisplayClassName, BindingFlags.NonPublic)!.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).ToList();

            //    var methodInfo = methodInfos.FirstOrDefault(x => x.Name == item.FullLambdaName);

            //    LambdaLinks.Add(methodInfo, item.Content);
            //}
        }

        private DbConnection? _connection;

        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;
        }

        internal async ValueTask EnsureConnectionEstablishedAsync(CancellationToken cancellationToken)
        {
            if (_connection is null)
            {
                _connection = new NpgsqlConnection(_connectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync(cancellationToken);
            }
        }
    }
}
