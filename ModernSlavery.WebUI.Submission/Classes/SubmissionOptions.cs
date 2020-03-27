using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Submission.Classes
{
    [Options("Web:Submission")]
    public class SubmissionOptions : IOptions
    {
        /// <summary>
        ///     Specifies how many reports an employer can edit.
        /// </summary>
        public int EditableReportCount { get; set; } = 4;

        /// <summary>
        ///     Specifies how many scopes an employer can edit.
        /// </summary>
        public int EditableScopeCount { get; set; } = 2;
    }
}