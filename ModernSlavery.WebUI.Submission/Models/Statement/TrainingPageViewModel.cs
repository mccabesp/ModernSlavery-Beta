using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class TrainingPageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public IList<TrainingViewModel> Training { get; set; }

        [MaxLength(50)]
        public string OtherTraining { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var otherTrainingCheckbox = Training.Single(x => x.Description.Equals("Other"));
            if (otherTrainingCheckbox.IsSelected && OtherTraining.IsNull())
                yield return new ValidationResult("Please provide other details");
        }

        public class TrainingViewModel
        {
            public short Id { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
