using CommandLine;

namespace ModernSlavery.Infrastructure.Azure.AppService
{
    public class AppServiceOptions : AzureOptions
    {
        [Option("app", Required = true, HelpText = "The name of the App Service")]
        public string AppService { get; set; }
    }
}
