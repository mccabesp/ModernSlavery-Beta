using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Viewing.Classes
{
    [Options("Web:Viewing")]
    public class ViewingOptions : IOptions
    {
        /// <summary>
        ///     Specifies how many reporting years the public can view or compare.
        /// </summary>
        public int ShowReportYearCount { get; set; } = 10;
    }
}