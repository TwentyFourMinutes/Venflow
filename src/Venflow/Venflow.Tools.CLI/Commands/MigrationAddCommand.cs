﻿using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Venflow.Tools.CLI.Commands
{
    internal class MigrationAddCommand : Command
    {
        private readonly static Argument<string> _migrationNameArgument = new Argument<string>("name", "The name of the new migration.");

        private readonly static Option<string?> _projectNameOption = new Option<string?>(new[] { "--project", "-p" }, "The name of the project, which contains the Database class.");
        private readonly static Option<string?> _contextNameOption = new Option<string?>(new[] { "--context", "-c" }, "The name of the Database class, from which the migration should be created.");
        private readonly static Option<string?> _assemblyPathOption = new Option<string?>(new[] { "--assembly", "-a" }, "The full path of the assembly.");

        internal MigrationAddCommand() : base("add", "Allows you to add a new migration.")
        {
            this.AddArgument(_migrationNameArgument);

            this.AddOption(_projectNameOption);
            this.AddOption(_contextNameOption);

            _assemblyPathOption.AddValidator(x => !File.Exists(x.GetValueOrDefault<string>()) ? "The given assembly file doesn't exist." : null);
            this.AddOption(_assemblyPathOption);

            this.Handler = new CommandHandler();
        }

        private class CommandHandler : CommandHandlerBase
        {
            protected override async Task HandleAsync()
            {
                var assemblyPath = GetOptionValue(_assemblyPathOption);

                var projectName = GetOptionValue(_projectNameOption);

                var projectPath = default(string);

                if (projectName is { })
                {
                    if (projectName.EndsWith(".csproj"))
                    {
                        if (File.Exists(projectName))
                        {
                            projectPath = Path.Combine(Directory.GetCurrentDirectory(), projectName);
                        }
                        else
                        {
                            throw new CommandException($"The current directory doesn't contain the file named '{projectName}'.");
                        }
                    }

                    if (projectPath is null &&
                        Directory.Exists(projectName))
                    {
                        var multipleMatches = false;

                        foreach (var fileName in Directory.EnumerateFiles(projectName, "*.csproj", SearchOption.TopDirectoryOnly))
                        {
                            if (multipleMatches)
                                throw new CommandException($"The directory '{Path.Combine(Directory.GetCurrentDirectory(), fileName)}' contains multiple '.csproj' files. Please be more specific by either providing the full path or the full relative path.");

                            projectPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                            multipleMatches = true;
                        }
                    }

                    if (projectPath is null)
                    {
                        throw new CommandException($"The project file couldn't be found. Searched location: '{Path.Combine(Directory.GetCurrentDirectory(), projectName)}'.");
                    }
                }
                else
                {
                    var multipleMatches = false;

                    foreach (var fileName in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.csproj", SearchOption.TopDirectoryOnly))
                    {
                        if (multipleMatches)
                            throw new CommandException($"The directory '{Path.Combine(Directory.GetCurrentDirectory(), fileName)}' contains multiple '.csproj' files. Please be more specific by either providing the full path or the full relative path.");

                        projectPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                        multipleMatches = true;
                    }

                    if (projectPath is null)
                    {
                        throw new CommandException($"The project file couldn't be found. Searched location: '{Path.Combine(Directory.GetCurrentDirectory())}'.");
                    }
                }

                Console.WriteLine("Building ...");

                if (assemblyPath is null)
                {
                    assemblyPath = await MSBuild.GetAssemblyPathFromProjectAsync(projectPath);
                }
                else
                {
                    await MSBuild.BuildProjectAsync(projectPath);
                }

                Console.WriteLine("Build successful.");

                var contextName = GetOptionValue(_contextNameOption);

                try
                {
                    var migrationAssembly = Assembly.LoadFrom(assemblyPath);

                    var designAssemblyPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Venflow.Design.dll");

                    if (!File.Exists(designAssemblyPath))
                        throw new CommandException($"The project `{migrationAssembly.FullName}` doesn't contain the 'Venflow.Design' NuGet package.");

                    var designAssembly = Assembly.LoadFrom(designAssemblyPath);

                    MigrationHandler migrationHandler;

                    if (contextName is { })
                    {
                        migrationHandler = new MigrationHandler(designAssembly, migrationAssembly, contextName);
                    }
                    else
                    {
                        migrationHandler = new MigrationHandler(designAssembly, migrationAssembly);
                    }

                    Console.WriteLine("Creating migration...");

                    var migrationName = GetArgumentValue(_migrationNameArgument);

                    if (!migrationHandler.TryCreateMigration(migrationName, out var migrationCode))
                    {
                        throw new CommandException("No changes to last migration detected.");
                    }

                    var migrationsFolder = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(projectPath), "Migrations")).FullName;

                    await File.WriteAllTextAsync(Path.Combine(migrationsFolder, DateTimeOffset.Now.ToString("ddMMyyyyhhmmss") + "_" + migrationName + ".cs"), migrationCode);

                    Console.WriteLine("Migration created.");
                }
                catch (Exception ex)
                {
                    throw new CommandException(ex.Message, ex);
                }
            }
        }
    }
}
