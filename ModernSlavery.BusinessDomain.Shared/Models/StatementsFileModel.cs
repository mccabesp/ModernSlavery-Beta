using System;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    public class StatementsFileModel
    {
        public long StatementId { get; set; }
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string DUNSNumber { get; set; }
        public string EmployerReference { get; set; }
        public string CompanyNumber { get; set; }
        public SectorTypes SectorType { get; set; }
        public ScopeStatuses? ScopeStatus { get; set; }
        public DateTime? ScopeStatusDate { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string StatementUrl { get; set; }
        public string ApprovingPerson { get; set; }
        public string Turnover { get; set; }
        public string Modifications { get; set; }
        public bool EHRCResponse { get; set; }

        public static TurnoverRanges GetTurnover(Statement statement)
        {
            return Enums.GetEnumFromRange<TurnoverRanges>(statement.MinTurnover, statement.MaxTurnover == null ? 0 : statement.MaxTurnover.Value);
        }
    }
}