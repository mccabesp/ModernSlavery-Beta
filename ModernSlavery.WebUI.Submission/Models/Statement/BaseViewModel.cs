using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public abstract class BaseViewModel: GovUkViewModel, IValidatableObject
    {
        [BindNever]
        public string BackUrl { get; set; }
        [BindNever]
        public string CancelUrl { get; set; }
        [BindRequired]
        public string ContinueUrl { get; set; }

        public abstract IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
    }
}
