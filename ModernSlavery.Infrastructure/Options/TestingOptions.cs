using ModernSlavery.SharedKernel.Options;

namespace ModernSlavery.Infrastructure.Options
{
    [Options("Testing")]

    public class TestingOptions : IOptions
    {
        public int MaxRecords { get; set; }
        public bool SkipSpamProtection { get; set; }

    }
}
