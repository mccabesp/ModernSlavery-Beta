using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public abstract class BaseViewModel : GovUkViewModel, IValidatableObject
    {

        public enum CommandType
        {
            Unknown = 0,
            Skip = 1,
            Continue = 2,
            Submit = 6,
            Search = 7,
            SearchNext = 8,
            SearchPrevious = 9,
            IncludeOrganisation = 10,
            RemoveOrganisation = 11,
            AddCountry = 12,
            RemoveCountry = 13
        }

        public enum Status
        {
            Unknown,
            [Description("Not Started")] Incomplete,
            [Description("In Progress")] InProgress,
            [Description("Completed")] Complete
        }

        [BindRequired]
        public bool Submitted { get; set; }

        [BindNever]
        public DateTime? DraftBackupDate { get; set; }

        [IgnoreMap]
        [BindNever]
        public string BackUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string SkipUrl { get; set; }

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

        [BindNever]
        public long OrganisationId { get; set; }
        [BindNever]
        public string OrganisationName { get; set; }

        public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();
            return validationResults;
        }

        public virtual Status GetStatus() => Status.Incomplete;
    }
}
