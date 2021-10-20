namespace Reflow.Playground
{
    public static class Program
    {
        public static void Main()
        {
            var db = new MyDatabase();
        }
    }

    public class MyDatabase : Database<MyDatabase>
    {
        public Table<Person> People { get; set; }
    }

    public class Person
    {

    }
}
