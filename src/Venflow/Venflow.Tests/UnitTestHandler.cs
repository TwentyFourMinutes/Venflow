using System.Threading.Tasks;
using Npgsql;
using Venflow.Shared;
using Venflow.Tests.Models;

namespace Venflow.Tests
{
    public class UnitTestHandler
    {
        private static UnitTestHandler _current;
        private static RelationDatabase _database;

        private static readonly object _buildLocker = new object();
        private static readonly TaskCompletionSource<bool> _waitHandle = new TaskCompletionSource<bool>();

        private UnitTestHandler()
        {
            NpgsqlConnection connection;
            NpgsqlCommand command;

            if (SecretsHandler.IsDevelopmentMachine())
            {
                connection = new NpgsqlConnection(SecretsHandler.GetConnectionString<UnitTestHandler>("Postgres").TrimEnd(';') + ";Enlist=true;Pooling=false;");

                connection.Open();

                command = new NpgsqlCommand(@"
                    DROP DATABASE IF EXISTS venflow_tests;
                    CREATE DATABASE venflow_tests OWNER venflow_tests;
                    ", connection);

                command.ExecuteNonQuery();

                command.Dispose();
                connection.Dispose();
            }

            connection = new NpgsqlConnection(_database.ConnectionString);
            connection.Open();

            command = new NpgsqlCommand(_createTablesCommand, connection);

            command.ExecuteNonQuery();

            command.Dispose();

            connection.ReloadTypes();

            connection.Close();
        }

        public static void Init(RelationDatabase database)
        {
            lock (_buildLocker)
            {
                if (_current is not null)
                    return;

                _database = database;
                _current = new UnitTestHandler();

                _waitHandle.SetResult(true);
            }
        }

        public static void Wait()
        {
            _waitHandle.Task.GetAwaiter().GetResult();
        }

        ~UnitTestHandler()
        {
            _database.ExecuteAsync("DROP DATABASE venflow_tests").GetAwaiter().GetResult();
            _database.GetConnection().Close();
        }

        private const string _createTablesCommand =
        @"ALTER SCHEMA public OWNER TO venflow_tests;

COMMENT ON SCHEMA public IS 'standard public schema';

CREATE EXTENSION ""uuid-ossp"";

CREATE TYPE public.postgre_enum AS ENUM(
    'foo',
    'bar'
);

ALTER TYPE public.postgre_enum OWNER TO venflow_tests;

CREATE TABLE public.""Blogs"" (
    ""Id"" integer NOT NULL,
    ""Topic"" text NOT NULL,
    ""UserId"" integer NOT NULL
);


ALTER TABLE public.""Blogs"" OWNER TO venflow_tests;

CREATE TABLE public.""EmailContents"" (
    ""Id"" integer NOT NULL,
    ""Content"" text NOT NULL,
    ""EmailId"" integer NOT NULL
);

ALTER TABLE public.""EmailContents"" OWNER TO venflow_tests;

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

ALTER TABLE public.""Emails"" OWNER TO venflow_tests;

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

ALTER TABLE public.""People"" OWNER TO venflow_tests;

ALTER TABLE public.""People"" ALTER COLUMN ""Id"" ADD GENERATED ALWAYS AS IDENTITY(
    SEQUENCE NAME public.""People_Id_seq""
    START WITH 0
    INCREMENT BY 1
    MINVALUE 0
    NO MAXVALUE
    CACHE 1
);

CREATE TABLE public.""UncommonTypes"" (
    ""Id"" uuid DEFAULT public.uuid_generate_v4() NOT NULL,
    ""CLRGuid"" uuid NOT NULL,
    ""NCLRGuid"" uuid,
    ""CLREnum"" integer NOT NULL,
    ""NCLREnum"" integer,
    ""CLRUInt64"" bigint NOT NULL,
    ""NCLRUInt64"" bigint,
    ""PostgreEnum"" public.postgre_enum NOT NULL,
    ""NPostgreEnum"" public.postgre_enum
);

ALTER TABLE public.""UncommonTypes"" OWNER TO venflow_tests;

CREATE TABLE public.""Users"" (
    ""Id"" integer NOT NULL,
    ""Name"" text NOT NULL
);

ALTER TABLE public.""Users"" OWNER TO venflow_tests;

ALTER TABLE ONLY public.""Blogs""
    ADD CONSTRAINT ""Blogs_pkey"" PRIMARY KEY(""Id"");

        ALTER TABLE ONLY public.""EmailContents""
    ADD CONSTRAINT ""EmailContents_pkey"" PRIMARY KEY(""Id"");

        ALTER TABLE ONLY public.""Emails""
    ADD CONSTRAINT ""Emails_pkey"" PRIMARY KEY(""Id"");

        ALTER TABLE ONLY public.""People""
    ADD CONSTRAINT ""People_pkey"" PRIMARY KEY(""Id"");

        ALTER TABLE ONLY public.""UncommonTypes""
    ADD CONSTRAINT ""UncommonTypes_pkey"" PRIMARY KEY(""Id"");

        ALTER TABLE ONLY public.""Users""
    ADD CONSTRAINT ""Users_pkey"" PRIMARY KEY(""Id"");

        ALTER TABLE ONLY public.""EmailContents""
    ADD CONSTRAINT ""FK_Emails_EmailContents"" FOREIGN KEY(""EmailId"") REFERENCES public.""Emails""(""Id"") ON DELETE CASCADE NOT VALID;

ALTER TABLE ONLY public.""Blogs""
    ADD CONSTRAINT ""FK_Users_Blogs"" FOREIGN KEY(""UserId"") REFERENCES public.""Users""(""Id"") ON DELETE CASCADE NOT VALID;

ALTER TABLE ONLY public.""Emails""
    ADD CONSTRAINT emails_people_id_fk FOREIGN KEY(""PersonId"") REFERENCES public.""People""(""Id"") ON DELETE CASCADE;";
    }
}
