using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Core.Options
{
    [Options("Email")]
    public class EmailOptions : IOptions
    {
        public string GEODistributionList { get; set; }
    }
}