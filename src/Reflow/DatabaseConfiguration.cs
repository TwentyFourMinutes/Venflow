namespace Reflow
{
    public class DatabaseConfiguration
    {
        public Action<object> Instantiater { get; }

        public DatabaseConfiguration(Action<object> instantiater)
        {
            Instantiater = instantiater;
        }
    }
}
