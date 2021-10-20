namespace Reflow
{
    public class DatabaseConfiguration
    {
        public Action<IDatabase> Instantiater { get; }

        public DatabaseConfiguration(Action<IDatabase> instantiater)
        {
            Instantiater = instantiater;
        }
    }
}
