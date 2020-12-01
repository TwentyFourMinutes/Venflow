using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace Venflow.Tools.CLI.Commands
{
    internal class MigrationAddCommand : Command
    {
        private readonly static Argument<string> _migrationNameArgument = new Argument<string>("name", "The name of the new migration.");
        internal MigrationAddCommand() : base("add", "Allows you to add a new migration.")
        {
            this.AddArgument(_migrationNameArgument);

            this.Handler = new CommandHandler();
        }

        private class CommandHandler : CommandHandlerBase
        {
            protected override async Task HandleAsync(CommandContext commandContext)
            {
                Console.WriteLine("Creating migration...");

                var migrationName = GetArgumentValue(_migrationNameArgument);

                if (!commandContext.MigrationHandler.TryCreateMigration(migrationName, out var migrationCode))
                {
                    throw new CommandException("No changes to last migration detected.");
                }

                var migrationsFolder = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(commandContext.ProjectPath), "Migrations")).FullName;

                await File.WriteAllTextAsync(Path.Combine(migrationsFolder, DateTimeOffset.Now.ToString("ddMMyyyyhhmmss") + "_" + migrationName + ".cs"), migrationCode);

                Console.WriteLine("Migration created.");
            }
        }
    }
}
