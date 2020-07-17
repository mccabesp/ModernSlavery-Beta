using System;
using System.Collections.Generic;
using System.Text;

namespace ModernSlavery.WebUI.Submission.Models
{
    public class YourStatementPageViewModel
    {
        public int Year { get; set; }
        public string OrganisationIdentifier { get; set; }

        public string StatementUrl { get; set; }

        // Dates are split in to 3 components here so that there are 3 field (Day/Month/Year)
        // if we use datetime components directly it did not seem to bind

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
        public int? StatementStartDay { get; set; }
        public int? StatementStartMonth { get; set; }
        public int? StatementStartYear { get; set; }

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
        public int? StatementEndDay { get; set; }
        public int? StatementEndMonth { get; set; }
        public int? StatementEndYear { get; set; }

        public string ApproverJobTitle { get; set; }
        public string ApproverFirstName { get; set; }
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
        public int? ApprovedDay { get; set; }
        public int? ApprovedMonth { get; set; }
        public int? ApprovedYear { get; set; }

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
