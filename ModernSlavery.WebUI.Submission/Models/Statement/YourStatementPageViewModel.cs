using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class YourStatementPageViewModel : GovUkViewModel, IValidatableObject
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a URL")] //this will only get triggeres through parseAndValidate
        [Url(ErrorMessage = "URL is not valid")]
        [MaxLength(255, ErrorMessage = "The web address (URL) cannot be longer than 255 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }

        // Dates are split in to 3 components here so that there are 3 field (Day/Month/Year)
        // if we use datetime components directly it did not seem to bind

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a date")]
        public DateTime? StatementStartDate
        {
            get
            {
                return ParseDate(StatementStartYear, StatementStartMonth, StatementStartDay);
            }
            set
            {
                if (value == null)
                {
                    StatementStartYear = null;
                    StatementStartMonth = null;
                    StatementStartDay = null;
                }
                else
                {
                    StatementStartYear = value.Value.Year;
                    StatementStartMonth = value.Value.Month;
                    StatementStartDay = value.Value.Day;
                }
            }
        }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? StatementStartDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? StatementStartMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? StatementStartYear { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a date")]
        public DateTime? StatementEndDate
        {
            get
            {
                return ParseDate(StatementEndYear, StatementEndMonth, StatementEndDay);
            }
            set
            {
                if (value == null)
                {
                    StatementEndYear = null;
                    StatementEndMonth = null;
                    StatementEndDay = null;
                }
                else
                {
                    StatementEndYear = value.Value.Year;
                    StatementEndMonth = value.Value.Month;
                    StatementEndDay = value.Value.Day;
                }
            }
        }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? StatementEndDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? StatementEndMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? StatementEndYear { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a job title")]
        public string ApproverJobTitle { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a first name")]
        public string ApproverFirstName { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a last name")]
        public string ApproverLastName { get; set; }

        public DateTime? ApprovedDate
        {
            get
            {
                return ParseDate(ApprovedYear, ApprovedMonth, ApprovedDay);
            }
            set
            {
                if (value == null)
                {
                    ApprovedYear = null;
                    ApprovedMonth = null;
                    ApprovedDay = null;
                }
                else
                {
                    ApprovedYear = value.Value.Year;
                    ApprovedMonth = value.Value.Month;
                    ApprovedDay = value.Value.Day;
                }
            }
        }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? ApprovedDay { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? ApprovedMonth { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Date format is incorrect")]
        public int? ApprovedYear { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationList = new List<ValidationResult>();

            var startDateList = new List<int?> { StatementStartDay, StatementStartMonth, StatementStartYear };
            if (startDateList.Any(x => x.HasValue) && !startDateList.Any(x => x.HasValue))
                validationList.Add(new ValidationResult("Please complete the Start Date"));

            var endDateList = new List<int?> { StatementEndDay, StatementEndMonth, StatementEndYear };
            if (endDateList.Any(x => x.HasValue) && !endDateList.Any(x => x.HasValue))
                validationList.Add(new ValidationResult("Please complete the End Date"));

            var approvalDateList = new List<int?> { ApprovedDay, ApprovedMonth, ApprovedYear };
            if (approvalDateList.Any(x => x.HasValue) && !approvalDateList.Any(x => x.HasValue))
                validationList.Add(new ValidationResult("Please complete the Approved Date"));

            var detailsList = new List<string> { ApproverFirstName, ApproverLastName, ApproverJobTitle };
            if (detailsList.Any(x => x.IsNull()) && !detailsList.Any(x => x.IsNull()))
                validationList.Add(new ValidationResult("Please complete First name, Last name, Job title"));

            return validationList;


        }

        private DateTime? ParseDate(int? year, int? month, int? day)
        {
            if (!year.HasValue || !month.HasValue || !day.HasValue)
                return null;

            DateTime result;
            if (DateTime.TryParse($"{year}-{month}-{day}", out result))
                return result;

            return null;
        }
    }
}
