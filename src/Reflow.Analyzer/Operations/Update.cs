using Reflow.Analyzer.Models;

namespace Reflow.Analyzer.Operations
{
    internal class Update
    {
        internal Command Command { get; }
        public Entity Entity { get; }

        private Update(Command command, Entity entity)
        {
            Command = command;
            Entity = entity;
        }

        internal static Update Construct(Database database, Command command)
        {
            return new Update(command, database.Entities[command.Entity]);
        }
    }
}
