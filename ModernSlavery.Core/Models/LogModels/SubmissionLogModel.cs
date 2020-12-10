using System;
using System.Linq;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Models.LogModels
{
    [Serializable]
    public class SubmissionLogModel
    {
        public DateTime StatusDate { get; set; }
        public StatementStatuses Status { get; set; }
        public string Details { get; set; }
        public SectorTypes Sector { get; set; }
        public long StatementId { get; set; }
        public string AccountingDate { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string Address { get; set; }
        public string CompanyNumber { get; set; }
        public string SicCodes { get; set; }
        public string StatementUrl { get; set; }
        public string ApprovingPerson { get; set; }
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }
        public string UserJobtitle { get; set; }
        public string UserEmail { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactOrganisation { get; set; }
        public string ContactPhoneNumber { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Browser { get; set; }
        public string SessionId { get; set; }

        public static SubmissionLogModel Create(Statement statement)
        {
            var status = statement.Statuses.FirstOrDefault(rs => rs.StatementId == statement.StatementId && rs.Status == statement.Status && rs.StatusDate == statement.StatusDate);

            return new SubmissionLogModel
            {
                StatusDate = statement.Created,
                Status = StatementStatuses.Submitted,
                Details = "",
                Sector = statement.Organisation.SectorType,
                StatementId = statement.StatementId,
                AccountingDate = statement.SubmissionDeadline.ToShortDateString(),
                OrganisationId = statement.OrganisationId,
                OrganisationName = statement.Organisation.OrganisationName,
                Address = statement.Organisation.LatestAddress?.GetAddressString(
                                    "," + Environment.NewLine),
                CompanyNumber = statement.Organisation.CompanyNumber,
                SicCodes = statement.Organisation.GetSicCodeIdsString(statement.StatusDate,
                                    "," + Environment.NewLine),
                StatementUrl = statement.StatementUrl,
                ApprovingPerson = statement.ApprovingPerson,
                UserFirstname = status.ByUser.Firstname,
                UserLastname = status.ByUser.Lastname,
                UserJobtitle = status.ByUser.JobTitle,
                UserEmail = status.ByUser.EmailAddress,
                ContactFirstName = status.ByUser.ContactFirstName,
                ContactLastName = status.ByUser.ContactLastName,
                ContactJobTitle = status.ByUser.ContactJobTitle,
                ContactOrganisation = status.ByUser.ContactOrganisation,
                ContactPhoneNumber = status.ByUser.ContactPhoneNumber
            };
        }
    }
}