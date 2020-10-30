using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public abstract class BaseViewModel: GovUkViewModel, IValidatableObject
    {
        public enum CommandType
        {
            Unknown=0,
            Cancel=1,
            Continue=2,
            SaveAndExit=3,
            DiscardAndExit=4,
            ExitNoChanges = 5,
            Submit = 6,
            Search=7,
            SearchNext = 8,
            SearchPrevious = 9,
            IncludeOrganisation = 10,
            RemoveOrganisation = 11,
            ToggleResults=12
        }

        public enum Status
        {
            Incomplete,
            InProgress,
            Complete
        }

        [BindRequired]
        public bool Submitted { get; set; }

        [BindRequired]
        public bool ReturnToReviewPage { get; set; }

        [BindNever]
        public DateTime? DraftBackupDate { get; set; }

        [IgnoreMap]
        [BindNever]
        public string BackUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string CancelUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string ContinueUrl { get; set; }

        [IgnoreMap]
        public abstract string PageTitle { get; }
        [IgnoreMap]
        public virtual string SubTitle { get; }

        public DateTime SubmissionDeadline { get; set; }

        [IgnoreMap]
        public int ReportingDeadlineYear => SubmissionDeadline.Year;

        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            return validationResults;
        }

        public virtual Status GetStatus() =>  Status.Incomplete;
    }
}
