using CommandLine;
using static ModernSlavery.Infrastructure.Azure.AppService.AppServiceManager;

namespace ModernSlavery.Infrastructure.Azure.AppService
{
    [Verb("SqlDatabase-SetEdition", HelpText = "Set the pricing edition the Sql database")]
    public class AppServiceSetPricingTierOptions : AppServiceOptions
    {
        [Option("tier", Required = true, HelpText = "The pricing tier to set")]
        public PricingTiers PricingTier { get; set; }
    }
}
