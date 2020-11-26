using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Venflow.Design;

namespace Venflow.Tools
{
    [Cmdlet(VerbsCommon.Add, "Migration")]
    public class AddMigrationCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = false, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public string ProjectName { get; set; }

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

            var migrationHandler = MigrationHandler.GetMigrationHandler(Assembly.LoadFrom(migrationAssemblyPath));

            WriteObject("yeet");
            WriteObject(Assembly.LoadFrom(migrationAssemblyPath).FullName);
        }
    }
}