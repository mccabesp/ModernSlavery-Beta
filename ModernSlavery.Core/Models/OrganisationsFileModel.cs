using System;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Core.Models
{
    public class OrganisationsFileModel
    {
        public long OrganisationId { get; set; }
        public string OrganisationReference { get; set; }
        public string OrganisationName { get; set; }
        public string CompanyNo { get; set; }
        public SectorTypes Sector { get; set; }
        public OrganisationStatuses Status { get; set; }
        public DateTime StatusDate { get; set; }
        public string StatusDetails { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressTownCity { get; set; }
        public string AddressCounty { get; set; }
        public string AddressCountry { get; set; }
        public string AddressPostCode { get; set; }
        public string SicCodes { get; set; }
        public DateTime? LatestRegistrationDate { get; set; }
        public RegistrationMethods? LatestRegistrationMethod { get; set; }
        public ScopeStatuses? ScopeStatus { get; set; }
        public DateTime? ScopeDate { get; set; }
        public DateTime Created { get; set; }

        public bool? IsGroupStatement { get; set; }
        public DateTime? LatestSubmission { get; set; }
        public DateTime? FirstSubmittedDate { get; set; }
        public int NumberOfStatements { get; set; }

        #region SecurityCode information

        public string SecurityCode { get; set; }
        public DateTime? SecurityCodeExpiryDateTime { get; set; }
        public DateTime? SecurityCodeCreatedDateTime { get; set; }

        #endregion
    }
}