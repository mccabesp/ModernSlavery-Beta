using System;
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
        public string EmployerName { get; set; }
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
    }
}