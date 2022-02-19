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

        public static int MyProperty2
        {
            get { return 0; }
            set { }
        }

        public static async Task Main()
        {
            var db = new MyDatabase();

            for (var i = 0; i < 2; i++)
            {
                var person = await db.People
                    .Query(people => $"SELECT {people:*} FROM {people} WHERE {people.Id} = {i}")
                    .ManyAsync();
            }

            var people = await db.People
                .Query<Email>(
                    (people, emails) =>
                        $@"SELECT {people:*} FROM {people}
                           JOIN {emails} ON {emails.PersonId} = {people.Id}
                           WHERE {people.Id} = {0}"
                )
                .Join(x => x.Emails)
                .ManyAsync();

            await db.People.InsertAsync(people);

            await db.People.DeleteAsync(people);
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
}
