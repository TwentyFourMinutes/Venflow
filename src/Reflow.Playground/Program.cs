using NpgsqlTypes;
using Reflow.Commands;
using Reflow.Modeling;
namespace Reflow.Playground
{
    public static class Program
    {
        public static int MyProperty
        {
            get { return 0; }
            set { }
        }

        public static async Task Main()
        {
            var db = new MyDatabase();
            Test(
                () =>
                {
                    var a = db.People
                        .Query(() => $"first {0}")
                        .TrackChanges()
                        .TrackChanges(false)
                        .SingleAsync();

                    var ad = db.People.Query(() => @$"second").TrackChanges(false).SingleAsync();
                }
            );

            for (var i = 0; i < 2; i++)
            {
                var ads = await db.People
                    .Query(() => @$"SELECT ""Id"", ""Name"" FROM ""People"" WHERE ""Id"" = {i}")
                    .SingleAsync();
            }

            var ad = await db.People.Query(() => @$"last").SingleAsync();
        }
        public static void Test(Action a) => Console.WriteLine(a);
    }

    public class MyDatabase : Database<MyDatabase>
    {
        public Table<Person> People { get; set; }

        public MyDatabase()
            : base(
                "User ID = venflow_tests; Password = venflow_tests; Server = 127.0.0.1; Port = 5432; Database = venflow_tests; "
            )
        { }
    }

    public class Person
    {
        public int Id { get; set; }
        public virtual string Name { get; set; }
    }

    public class PersonConfiguration : IEntityConfiguration<Person>
    {
        void IEntityConfiguration<Person>.Configure(IEntityBuilder<Person> entityBuilder)
        {
            entityBuilder.MapTable("people");

            entityBuilder.MapId(x => x.Id);

            entityBuilder.Column(x => x.Name).HasName("name").HasType(NpgsqlDbType.Varchar);
        }
    }
}
