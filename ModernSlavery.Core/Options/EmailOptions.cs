using ModernSlavery.Core.Attributes;

namespace ModernSlavery.Core.Options
{
    [Options("Email")]
    public class EmailOptions : IOptions
    {
        public string AdminDistributionList { get; set; }
    }
}