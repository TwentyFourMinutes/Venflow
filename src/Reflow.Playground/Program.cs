using NpgsqlTypes;
using Reflow.Modeling;

namespace Reflow.Playground
{
    public static class Program
    {
        public static void Main()
        {
            var db = new MyDatabase();

            db.People.Query(() => $"select * from people where id = {0}");
        }
    }

    public class MyDatabase : Database<MyDatabase>
    {
        public Table<Person> People { get; set; }

        public MyDatabase() : base("")
        {

        }
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

            entityBuilder.Column(x => x.Name)
                         .HasName("name")
                         .HasType(NpgsqlDbType.Varchar);
        }
    }
}
