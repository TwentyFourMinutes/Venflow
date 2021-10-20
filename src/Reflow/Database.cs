namespace Reflow
{
    public class Database<T> : IDatabase where T : Database<T>
    {
        private static readonly DatabaseConfiguration _configuration;

        static Database()
        {
            var configurations = (Dictionary<Type, DatabaseConfiguration>)typeof(T).Assembly.GetType("Reflow.DatabaseConfigurations")!.GetField("Configurations")!.GetValue(null)!;

            lock (configurations)
            {
                if (!configurations.Remove(typeof(T), out _configuration!))
                {
                    throw new InvalidOperationException();
                }

#pragma warning disable CS0728 // Possibly incorrect assignment to local which is the argument to a using or lock statement
                if (configurations.Count == 0)
                    configurations = null!;
#pragma warning restore CS0728 // Possibly incorrect assignment to local which is the argument to a using or lock statement
            }
        }

        public Database()
        {
            _configuration.Instantiater.Invoke(this);
        }
    }

    public interface IDatabase
    {

    }
}
