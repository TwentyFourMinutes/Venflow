using System.CommandLine;
using System.Threading.Tasks;

namespace Venflow.Tools.CLI.Commands
{
    internal class MigrationAddCommand : Command
    {
        private readonly static Argument<string> _migrationNameArgument = new Argument<string>("name", "The name of the new migration.");

        private readonly static Option _projectNameOption = new Option<string?>(new[] { "project", "p" }, "The name of the project, which contains the Database class.");
        private readonly static Option _contextNameOption = new Option<string?>(new[] { "context", "c" }, "The name of the Database class, from which the migration should be created.");

        internal MigrationAddCommand() : base("add", "Allows you to add a new migration.")
        {
            this.AddArgument(_migrationNameArgument);

            this.AddOption(_projectNameOption);
            this.AddOption(_contextNameOption);

            this.Handler = new CommandHandler();
        }

        private class CommandHandler : CommandHandlerBase
        {
            protected override Task HandleAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}
