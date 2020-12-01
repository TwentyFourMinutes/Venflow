using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading.Tasks;
using Venflow.Tools.CLI.Commands;

namespace Venflow.Tools.CLI
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var app = new AppRootCommand();

            while (true)
            {
                await app.InvokeAsync(Console.ReadLine());
            }
        }
    }

    internal class AppRootCommand : RootCommand
    {
        internal readonly static Option<string?> ProjectNameOption = new Option<string?>(new[] { "--project", "-p" }, "The name of the project, which contains the Database class.");
        internal readonly static Option<string?> ContextNameOption = new Option<string?>(new[] { "--context", "-c" }, "The name of the Database class, from which the migration should be created.");
        internal readonly static Option<string?> AssemblyPathOption = new Option<string?>(new[] { "--assembly", "-a" }, "The full path of the assembly.");

        internal AppRootCommand()
        {
            Name = "vf";
            Description = "Contains a set of code-first tools for Venflow.";

            AddAlias("venflow");
            AddCommand(new MigrationCommand());

            AddGlobalOption(ProjectNameOption);
            AddGlobalOption(ContextNameOption);

            AssemblyPathOption.AddValidator(x => !File.Exists(x.GetValueOrDefault<string>()) ? "The given assembly file doesn't exist." : null);
            AddGlobalOption(AssemblyPathOption);
        }
    }
}
