using ModernSlavery.Core.Attributes;
using ModernSlavery.Core.Options;

namespace ModernSlavery.WebUI.Submission.Classes
{
    [Options("Web:Submission")]
    public class SubmissionOptions : IOptions
    {
        /// <summary>
        ///     Specifies how many reports an organisation can edit.
        /// </summary>
        public int EditableReportCount { get; set; } = 4;

        /// <summary>
        ///     Specifies how many scopes an organisation can edit.
        /// </summary>
        public int EditableScopeCount { get; set; } = 2;
    }
}