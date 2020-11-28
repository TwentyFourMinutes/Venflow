using System;
using System.CommandLine;
using System.Threading.Tasks;
using Venflow.Tools.CLI.Commands;

namespace Venflow.Tools.CLI
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var command = new RootCommand
            {
                Name = "vf",
                Description = "Contains a set of code-first tools for Venflow."
            };

            command.AddAlias("venflow");

            command.AddCommand(new MigrationCommand());

            while (true)
            {
                await command.InvokeAsync(Console.ReadLine());
            }
        }
    }
}
