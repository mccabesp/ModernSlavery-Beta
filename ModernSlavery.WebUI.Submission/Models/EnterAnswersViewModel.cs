using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Management.WebSites.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;

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
        public List<string> SelectedReasonOptions { get; set; } = new List<string>();

        internal IEnumerable<string> FriendlyReasonOptions
        {
            get
            {
                if (SelectedReasonOptions.Contains("Other"))
                {
                    return SelectedReasonOptions
                        .Where(i => i != "Other")
                        .Append(OtherReason);
                }
                else
                    return SelectedReasonOptions;
            }
        }

        [Required]
        [MaxLength(256)]
        public string OtherReason { get; set; }

        [Required]
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

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
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