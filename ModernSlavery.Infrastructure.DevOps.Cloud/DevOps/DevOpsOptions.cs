using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.DevOps
{
    public class DevOpsOptions
    {
        [Option("org", Required = true, HelpText = "The name of the Azure DevOps organisation")]
        public string Organisation { get; set; }

        [Option("pat", Required = true, HelpText = "The personal access token of the Azure DevOps administrator")]
        public string PersonalAccessToken { get; set; }
    }
}
