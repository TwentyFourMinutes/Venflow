using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using EnvDTE;
using EnvDTE80;

namespace Venflow.Tools
{
    [Cmdlet(VerbsCommon.Add, "Migration")]
    public class AddMigrationCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string ProjectName { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string ContextName { get; set; }

        [Parameter(Mandatory = false, Position = 3, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public bool NoBuild { get; set; }

        protected override void ProcessRecord()
        {
            var dte = (DTE2)base.GetVariableValue("DTE");

            var migrationProject = default(Project);

            if (string.IsNullOrWhiteSpace(ProjectName))
            {
                var startupProjects = (object[])dte.Solution.SolutionBuild.StartupProjects;

                if (startupProjects.Length != 1)
                {
                    WriteError(new ErrorRecord(new Exception("No startup project was set in the solution."), string.Empty, ErrorCategory.InvalidArgument, null));

                    return;
                }

                var startupProjectName = (string)startupProjects[0];

                foreach (Project project in dte.Solution.Projects)
                {
                    if (project.UniqueName == startupProjectName)
                    {
                        migrationProject = project;
                    }
                }

                if (migrationProject is null)
                    WriteError(new ErrorRecord(new Exception("No startup project was set in the solution."), string.Empty, ErrorCategory.InvalidArgument, null));
            }
            else
            {
                foreach (Project project in dte.Solution.Projects)
                {
                    if (project.Name == ProjectName)
                    {
                        migrationProject = project;
                    }
                }

                if (migrationProject is null)
                {
                    foreach (Project project in dte.Solution.Projects)
                    {
                        if (project.Name.Equals(ProjectName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            migrationProject = project;
                        }
                    }
                }

                if (migrationProject is null)
                    WriteError(new ErrorRecord(new Exception($"No project in this solution found with the name '{ProjectName}'."), string.Empty, ErrorCategory.InvalidArgument, null));
            }

            var migrationAssemblyPath = Path.Combine(Path.GetDirectoryName(migrationProject.FullName), (string)migrationProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value, (string)migrationProject.Properties.Item("OutputFileName").Value);

            var args = "migrations add " + Name;

            args += $" -a \"{migrationAssemblyPath}\"";
            args += $" -p \"{migrationProject.FullName}\"";

            if (NoBuild)
            {
                args += " --no-build";
            }

            if (!string.IsNullOrWhiteSpace(ContextName))
            {
                args += $" -c {ContextName}";
            }

            CliTools.Execute(Path.GetDirectoryName(migrationProject.FullName), args, msg => WriteObject(msg), msg => WriteError(new ErrorRecord(new Exception(msg), string.Empty, ErrorCategory.NotSpecified, null)));
        }
    }

    internal static class CliTools
    {
        internal static void Execute(string workingDirectory, string args, Action<string> onOutput, Action<string> onError)
        {
            var processInfo = new ProcessStartInfo
            {
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = "dotnet vf",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var process = new System.Diagnostics.Process();

            process.StartInfo = processInfo;

            process.OutputDataReceived += (_, e) => onOutput.Invoke(e.Data);
            process.ErrorDataReceived += (_, e) => onError.Invoke(e.Data);

            process.Start();

            process.WaitForExit();

            process.Dispose();
        }
    }
}