using System.Management.Automation;

namespace Venflow.Tools
{
    [Cmdlet(VerbsCommon.Get, "Salutation")]
    public class GetSalutationCmdlet : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true,
            Position = 0,
            HelpMessage = "Name to get salutation for.")]
        [Alias("Person", "FirstName")]
        public string[] Name { get; set; }

        protected override void ProcessRecord()
        {
            WriteVerbose("yeet");
        }
    }
}