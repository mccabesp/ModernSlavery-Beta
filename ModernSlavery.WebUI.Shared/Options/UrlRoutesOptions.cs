using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ModernSlavery.Core.SharedKernel.Options;

namespace ModernSlavery.WebUI.Shared.Options
{
    [Options("UrlRoutes")]
    public class UrlRouteOptions : Dictionary<string, string>
    {

        public UrlRouteOptions() : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        public enum Routes
        {
            [EnumMember(Value = "AccountSignOut")] AccountSignOut,
            [EnumMember(Value = "AccountHome")] AccountHome,
            [EnumMember(Value = "AccountVerifyEmail")] AccountVerifyEmail,
            [EnumMember(Value = "AccountNewPassword")] AccountNewPassword,
            [EnumMember(Value = "AdminHome")] AdminHome,
            [EnumMember(Value = "AdminReviewRequest")] AdminReviewRequest,
            [EnumMember(Value = "Done")] Done,
            [EnumMember(Value = "RegistrationAdd")] RegistrationAdd,
            [EnumMember(Value = "RegistrationPINSent")] RegistrationPINSent,
            [EnumMember(Value = "RegistrationActivate")] RegistrationActivate,
            [EnumMember(Value = "RegistrationRemove")] RegistrationRemove,
            [EnumMember(Value = "SubmissionHome")] SubmissionHome,
            [EnumMember(Value = "SearchHome")] SearchHome,
            [EnumMember(Value = "ViewingActionHub")] ViewingActionHub,
            [EnumMember(Value = "ViewingDownloads")] ViewingDownloads,
            [EnumMember(Value = "ViewingEmployer")] ViewingEmployer,
            [EnumMember(Value = "ViewingReport")] ViewingReport,
            [EnumMember(Value = "ViewingGuidance")] ViewingGuidance,
            [EnumMember(Value = "ViewingHome")] ViewingHome
        }
    }
}
