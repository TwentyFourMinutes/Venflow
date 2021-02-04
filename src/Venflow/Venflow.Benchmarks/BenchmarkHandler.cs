using System.Threading.Tasks;
using Npgsql;
using Venflow.Benchmarks.Models.Configurations;
using Venflow.Shared;

namespace Venflow.Benchmarks
{
    public class BenchmarkHandler
    {
        private static BenchmarkHandler _current;
        private static BenchmarkDb _database;

        private static readonly object _buildLocker = new object();
        private static readonly TaskCompletionSource<bool> _waitHandle = new TaskCompletionSource<bool>();

        private BenchmarkHandler()
        {
            NpgsqlConnection connection;
            NpgsqlCommand command;

            if (SecretsHandler.IsDevelopmentMachine("Benchmarks"))
            {
                connection = new NpgsqlConnection(SecretsHandler.GetConnectionString<BenchmarkHandler>("Postgres").TrimEnd(';') + ";Enlist=true;Pooling=false;");

                connection.Open();

                command = new NpgsqlCommand(@"
                    DROP DATABASE IF EXISTS venflow_benchmarks;
                    CREATE DATABASE venflow_benchmarks OWNER venflow_benchmarks;
                    ", connection);

                command.ExecuteNonQuery();

                command.Dispose();
                connection.Dispose();
            }

            try
            {
                connection = _database.GetConnection();
                connection.Open();

                command = new NpgsqlCommand(_createTablesCommand, connection);

                command.ExecuteNonQuery();

                command.Dispose();

                connection.ReloadTypes();

                connection.Close();
            }
            catch
            {
                // We are running on a different Framework version.
            }
        }

        public static void Init(BenchmarkDb database)
        {
            lock (_buildLocker)
            {
                if (_current != null)
                    return;

                _database = database;
                _current = new BenchmarkHandler();

                _waitHandle.SetResult(true);
            }
        }

        public static void Wait()
        {
            _waitHandle.Task.GetAwaiter().GetResult();
        }

        ~BenchmarkHandler()
        {
            _database.GetConnection().Close();
        }

        private const string _createTablesCommand =
@"ALTER SCHEMA public OWNER TO venflow_benchmarks;

COMMENT ON SCHEMA public IS 'standard public schema';

CREATE EXTENSION ""uuid-ossp"";

CREATE TABLE public.""EmailContents"" (
    ""Id"" integer NOT NULL,
    ""Content"" text NOT NULL,
    ""EmailId"" integer NOT NULL
);

ALTER TABLE public.""EmailContents"" OWNER TO venflow_benchmarks;

ALTER TABLE public.""EmailContents"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY(
    SEQUENCE NAME public.""EmailContents_Id_seq""
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.""Emails"" (
    ""Id"" integer NOT NULL,
    ""Address"" text NOT NULL,
    ""PersonId"" integer NOT NULL
);

ALTER TABLE public.""Emails"" OWNER TO venflow_benchmarks;

ALTER TABLE public.""Emails"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY(
    SEQUENCE NAME public.""Emails_Id_seq""
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.""People"" (
    ""Id"" integer NOT NULL,
    ""Name"" text
);

ALTER TABLE public.""People"" OWNER TO venflow_benchmarks;

ALTER TABLE public.""People"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY(
    SEQUENCE NAME public.""People_Id_seq""
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);

ALTER TABLE ONLY public.""EmailContents""
    ADD CONSTRAINT ""EmailContents_pkey"" PRIMARY KEY(""Id"");

ALTER TABLE ONLY public.""Emails""
    ADD CONSTRAINT ""Emails_pkey"" PRIMARY KEY(""Id"");

ALTER TABLE ONLY public.""People""
    ADD CONSTRAINT ""People_pkey"" PRIMARY KEY(""Id"");

ALTER TABLE ONLY public.""EmailContents""
    ADD CONSTRAINT ""FK_Emails_EmailContents"" FOREIGN KEY(""EmailId"") REFERENCES public.""Emails""(""Id"") ON DELETE CASCADE NOT VALID;

ALTER TABLE ONLY public.""Emails""
    ADD CONSTRAINT emails_people_id_fk FOREIGN KEY(""PersonId"") REFERENCES public.""People""(""Id"") ON DELETE CASCADE;";
    }
}
