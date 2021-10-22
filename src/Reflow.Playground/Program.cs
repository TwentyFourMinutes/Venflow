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

    }
}
