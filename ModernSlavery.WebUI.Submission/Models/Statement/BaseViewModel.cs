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
            AddOrganisation=7,
            RemoveOrganisation = 8
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

        public string OrganisationName { get; set; }

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);

        public virtual bool IsComplete() => false;
    }
}
