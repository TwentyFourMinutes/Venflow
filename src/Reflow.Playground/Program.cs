using Reflow.Modeling;

namespace Reflow.Playground
{
    public static class Program
    {
        public static async Task Main()
        {
            var db = new MyDatabase();

            for (var i = 0; i < 3; i++)
            {
                var people = await db.People
                    .Query(people => $"SELECT {people:*} FROM {people} WHERE {people.Id} = {1}")
                    .Caching()
                    .SingleAsync();

                people = await db.People
                    .Query(people => $"SELECT {people:*} FROM {people} WHERE {people.Id} = 1")
                    .Caching()
                    .SingleAsync();

                people = await db.People
                    .QueryRaw(() => @"SELECT * FROM ""People"" WHERE ""Id"" = 0")
                    .Caching()
                    .SingleAsync();
            }
        }

        public static void Test(Action a) => Console.WriteLine(a);
    }

    public class MyDatabase : Database<MyDatabase>
    {
        public Table<Person> People { get; set; }
        public Table<Email> Emails { get; set; }

        public MyDatabase()
            : base(
                "User ID = venflow_tests; Password = venflow_tests; Server = 127.0.0.1; Port = 5432; Database = venflow_tests; "
            ) { }
    }

    public class Person
    {
        public int Id { get; set; }
        public virtual string Name { get; set; } = null!;
        public DateTime DefaultValue { get; set; }

        public IList<Email> Emails { get; }

        public Person()
        {
            Emails = new List<Email>();
        }
    }

    public class Email
    {
        public int Id { get; set; }
        public string Address { get; set; } = null!;

        public int PersonId { get; set; }
        public Person Person { get; set; } = null!;
    }

    public class PersonConfiguration : IEntityConfiguration<Person>
    {
        void IEntityConfiguration<Person>.Configure(IEntityBuilder<Person> entityBuilder)
        {
            entityBuilder.MapTable("People");

            entityBuilder.Column(x => x.Name).HasName("Name");

            entityBuilder
                .HasMany(x => x.Emails)
                .WithOne(x => x.Person)
                .UsingForeignKey(x => x.PersonId);
        }
    }

    [GeneratedKey<Guid>]
    public partial struct Key<T> { }
}
