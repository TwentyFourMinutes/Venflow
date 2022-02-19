using Reflow.Analyzer.Models;

namespace Reflow.Analyzer.Operations
{
    internal class Delete
    {
        internal Command Command { get; }
        public Entity Entity { get; }

        private Delete(Command command, Entity entity)
        {
            Command = command;
            Entity = entity;
        }

        internal static Delete Construct(Database database, Command command)
        {
            return new Delete(command, database.Entities[command.Entity]);
        }
    }
}
