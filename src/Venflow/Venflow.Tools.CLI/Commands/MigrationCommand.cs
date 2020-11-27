using System.CommandLine;

namespace Venflow.Tools.CLI.Commands
{
    internal class MigrationCommand : Command
    {
        internal MigrationCommand() : base("migrations", "Allows you to perform migration actions.")
        {
            this.AddAlias("mig");

            this.AddCommand(new MigrationAddCommand());
        }
    }
}
