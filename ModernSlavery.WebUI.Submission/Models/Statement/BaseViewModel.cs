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
        [BindNever]
        public bool CanRevertToOriginal { get; set; }

        [BindNever]
        public DateTime? DraftBackupDate { get; set; }

        [IgnoreMap]
        [BindNever]
        public string BackUrl { get; set; }

        [IgnoreMap]
        [BindNever]
        public string CancelUrl { get; set; }

        [IgnoreMap]
        [BindRequired]
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
        public virtual bool IsEmpty() => false;
    }
}
