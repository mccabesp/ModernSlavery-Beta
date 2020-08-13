using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models
{
    public class DownloadModel
    {
        public string EmployerName { get; set; }
        public string Address { get; set; }
        public string CompanyNumber { get; set; }
        public string SicCodes { get; set; }
        public string StatementUrl { get; set; }
        public string ApprovingPerson { get; set; }
        public string Turnover { get; set; }
        public string CurrentName { get; set; }
        public bool SubmittedAfterTheDeadline { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime DateSubmitted { get; set; }

        public static DownloadModel Create(Statement statement)
        {
            return new DownloadModel
            {
                EmployerName = statement.Organisation.GetName(statement.StatusDate)?.Name ??
                               statement.Organisation.OrganisationName,
                Address = statement.Organisation.GetAddressString(statement.StatusDate,
                    delimiter: "," + Environment.NewLine),
                CompanyNumber = statement.Organisation?.CompanyNumber,
                SicCodes = statement.Organisation?.GetSicCodeIdsString(statement.StatusDate, "," + Environment.NewLine),
                StatementUrl = statement.StatementUrl,
                ApprovingPerson = statement.ApprovingPerson,
                Turnover = statement.GetAttribute<DisplayAttribute>().Name,
                CurrentName = statement.Organisation?.OrganisationName,
                SubmittedAfterTheDeadline = statement.IsLateSubmission(),
                DueDate = statement.SubmissionDeadline.AddYears(1),
                DateSubmitted = statement.Modified
            };
        }
    }
}