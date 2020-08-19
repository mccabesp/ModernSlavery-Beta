using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class EnterAnswersViewModel : IValidatableObject
    {
        public string[] ReasonOptions = new[]
        {
         "Its turnover or budget is less than £36 million per year",
         "It does not provide goods or services",
         "It does not have a business presence in the UK",
         "It is in administration or liquidation, has closed or is dormant, or has merged with another organisation",
         "Other"
        };

        public List<string> SelectedReasonOptions { get; set; } = new List<string>();

        [MaxLength(256)]
        public string OtherReason { get; set; }

        public string TurnOver { get; set; }

        public string Reason
        {
            get
            {
                var selectedReasonOptions = new List<string>(SelectedReasonOptions.Where(s => !string.IsNullOrWhiteSpace(s)));

                if (selectedReasonOptions.Contains("Other"))
                {
                    selectedReasonOptions.Remove("Other");
                    selectedReasonOptions.Add(OtherReason);
                }

                return selectedReasonOptions.ToDelimitedString(Environment.NewLine);
            }
            set
            {
                var selectedReasonOptions = new List<string>(value.SplitI(Environment.NewLine).Where(s => !string.IsNullOrWhiteSpace(s)));

                //Set the selected types
                SelectedReasonOptions.Clear();
                for (int i = selectedReasonOptions.Count - 1; i >= 0; i--)
                {
                    if (ReasonOptions.ContainsI(selectedReasonOptions[i]))
                    {
                        SelectedReasonOptions.Add(selectedReasonOptions[i]);
                        selectedReasonOptions.RemoveAt(i);
                    }
                }
                OtherReason = selectedReasonOptions.ToDelimitedString(Environment.NewLine);
                if (!string.IsNullOrWhiteSpace(OtherReason)) SelectedReasonOptions.Add("Other");
            }
        }

        public string ReadGuidance { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string JobTitle { get; set; }

        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; }

        public bool HasName => !string.IsNullOrEmpty(FirstName + LastName);

        public string FullName => $"{FirstName} {LastName}";

        public bool? HasReadGuidance()
        {
            if (!string.IsNullOrWhiteSpace(ReadGuidance)) return ReadGuidance.ToBoolean();

            return null;
        }

        public bool RequiresEmailConfirmation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (!SelectedReasonOptions.Any())
                validationResults.AddValidationError(2115, nameof(SelectedReasonOptions));

            if (SelectedReasonOptions.Contains("Other") && string.IsNullOrWhiteSpace(OtherReason))
                validationResults.AddValidationError(2117, nameof(OtherReason));

            return validationResults;
        }
    }


}