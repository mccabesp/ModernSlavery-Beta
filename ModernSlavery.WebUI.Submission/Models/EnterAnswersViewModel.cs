using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;

namespace ModernSlavery.WebUI.Submission.Models
{
    [Serializable]
    public class EnterAnswersViewModel
    {
        public string[] ReasonOptions = new[]
        {
         "Its turnover or budget is less than £36 million per year",
         "It does not provide goods or services",
         "It does not have a business presence in the UK",
         "It is in administration or liquidation, has closed or is dormant, or has merged with another organisation",
         "Other"
        };

        [MinLength(1)]
        [IgnoreText]
        public List<string> SelectedReasonOptions { get; set; } = new List<string>();

        public IEnumerable<string> FriendlyReasonOptions
        {
            get
            {
                if (SelectedReasonOptions.Contains("Other"))
                {
                    return SelectedReasonOptions
                        .Where(i => i != "Other")
                        .Append(OtherReason);
                }
                if (SelectedReasonOptions.Contains("Its turnover or budget is less than £36 million per year"))
                {
                    return SelectedReasonOptions
                        .Append($"Its annual turnover or budget is £{TurnOver}");

                }
                else
                    return SelectedReasonOptions;
            }
        }

        [Required]
        [MaxLength(ReasonMaxLength)]
        [Text]
        public string OtherReason { get; set; }

        public const int ReasonMaxLength = 200;

        [Required]
        [Text]
        public string TurnOver { get; set; }

        [Text]
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
                var selectedReasonOptions = new List<string>(value.SplitI(Environment.NewLine.ToCharArray()).Where(s => !string.IsNullOrWhiteSpace(s)));

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

        [Required]
        [Text]
        public string FirstName { get; set; }

        [Required]
        [Text]
        public string LastName { get; set; }

        [Required]
        [Text]
        public string JobTitle { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        public bool HasName => !string.IsNullOrEmpty(FirstName + LastName);

        public string FullName => $"{FirstName} {LastName}";

        public bool RequiresEmailConfirmation { get; set; }

        [BindNever]
        public string BackUrl { get; set; }

        [BindNever]
        public bool UserIsRegistered { get; set; }
    }
}