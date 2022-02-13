using System.Data;
using System.Data.Common;
using System.Reflection;
using Npgsql;
using Reflow.Lambdas;

namespace Reflow
{
    public class Database<T> : IDatabase where T : Database<T>
    {
        private static readonly DatabaseConfiguration _configuration;

        static Database()
        {
            AssemblyRegister.Assembly ??= typeof(T).Assembly;

            var configurationsField = AssemblyRegister.Assembly.GetType(
                "Reflow.DatabaseConfigurations"
            )!.GetField("Configurations")!;

            var configurations =
                (Dictionary<Type, DatabaseConfiguration>)configurationsField.GetValue(null)!;

            lock (configurations)
            {
                var databaseType = typeof(T);

                if (
                    !configurations.Remove(databaseType, out _configuration!)
                    || _configuration.LambdaLinks is null
                )
                {
                    throw new InvalidOperationException();
                }

                var queries = new Dictionary<MethodInfo, ILambdaLinkData>();

                for (var linkIndex = 0; linkIndex < _configuration.LambdaLinks.Length; linkIndex++)
                {
                    var link = _configuration.LambdaLinks[linkIndex];

                    MethodInfo? method = null;

                    if (link.HasClosure)
                    {
                        var nestedTypes = link.ClassType.GetNestedTypes(BindingFlags.NonPublic);

                        for (
                            var nestedTypeIndex = 0;
                            nestedTypeIndex < nestedTypes.Length;
                            nestedTypeIndex++
                        )
                        {
                            var nestedType = nestedTypes[nestedTypeIndex];

                            if (
                                !nestedType.Name.StartsWith("<>c__DisplayClass")
                                || !nestedType.Name.EndsWith(
                                    (link.LambdaIndex >> sizeof(ushort) * 8).ToString()
                                )
                            )
                                continue;

                            var tempMethod = nestedType.GetMethods(
                                BindingFlags.NonPublic | BindingFlags.Instance
                            )[link.LambdaIndex & ushort.MaxValue];

                            if (
                                tempMethod is null
                                || !tempMethod.Name.StartsWith("<" + link.IdentifierName + ">b__")
                            )
                                continue;

                            method = tempMethod;
                            break;
                        }

                        if (method is null)
                            throw new InvalidOperationException();
                    }
                    else
                    {
                        var nestedType = link.ClassType.GetNestedType(
                            "<>c",
                            BindingFlags.NonPublic
                        )!;

                        var methods = nestedType.GetMethods(
                            BindingFlags.Instance | BindingFlags.NonPublic
                        );

                        var tempMethod = methods[link.LambdaIndex];

                        if (
                            tempMethod is null
                            || !tempMethod.Name.StartsWith("<" + link.IdentifierName + ">b__")
                        )
                            continue;

                        method = tempMethod;
                    }

                    queries.Add(method, link.Data);
                }

                _configuration.LambdaLinks = null;
                _configuration.Queries = queries;

                if (configurations.Count == 0)
                {
                    configurationsField.SetValue(null, null);
                }
            }
        }

        /// <inheritdoc/>
        public DbConnection? Connection { get; private set; }
        DbConnection? IDatabase.Connection => Connection;

        DatabaseConfiguration IDatabase.Configuration => _configuration;

        private readonly string _connectionString;

        public Database(string connectionString)
        {
            _connectionString = connectionString;

            _configuration.Instantiater.Invoke(this);
        }

        internal ValueTask EnsureValidConnection(CancellationToken cancellationToken)
        {
            Connection ??= new NpgsqlConnection(_connectionString);

            if (Connection.State == ConnectionState.Open)
            {
                return default;
            }
            else if (Connection.State == ConnectionState.Closed)
            {
                return new ValueTask(Connection.OpenAsync(cancellationToken));
            }
            else
            {
                throw new InvalidOperationException(
                    $"The current connection state is invalid. Expected: '{ConnectionState.Open}' or '{ConnectionState.Closed}'. Actual: '{Connection.State}'."
                );
            }
        }

        ValueTask IDatabase.EnsureValidConnection(CancellationToken cancellationToken) =>
            EnsureValidConnection(cancellationToken);

        TData IDatabase.GetQueryData<TData>(MethodInfo method)
        {
            return (TData)_configuration.Queries[method];
        }
    }

    internal interface IDatabase
    {
        DatabaseConfiguration Configuration { get; }
        DbConnection? Connection { get; }
        ValueTask EnsureValidConnection(CancellationToken cancellationToken);

        TData GetQueryData<TData>(MethodInfo method) where TData : ILambdaLinkData;
    }
}
