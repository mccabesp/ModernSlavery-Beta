using System;
using System.Collections.Generic;
using AutoMapper;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class OrganisationSearchModel
    {
        public class OrganisationSearchModelMapperProfile : Profile
        {
            public OrganisationSearchModelMapperProfile()
            {
                CreateMap<OrganisationSearchModel, OrganisationSearchModel>();
            }
        }

        public override bool Equals(object obj)
        {
            var target = obj as OrganisationSearchModel;
            return target != null && target.SearchDocumentKey == SearchDocumentKey;
        }

        public override int GetHashCode()
        {
            return SearchDocumentKey.GetHashCode();
        }

        #region Search Properties
        public virtual string SearchDocumentKey { get; set; }
        public virtual string PartialNameForSuffixSearches { get; set; }
        public virtual string PartialNameForCompleteTokenSearches { get; set; }
        public virtual string[] Abbreviations { get; set; }
        public virtual DateTime Timestamp { get; } = VirtualDateTime.Now;

        #endregion

        #region General Properties
        public virtual long? StatementId { get; set; }
        public virtual long ParentOrganisationId { get; set; }
        public virtual int? SubmissionDeadlineYear { get; set; }
        public virtual string OrganisationName { get; set; }

        public virtual int? OrganisationType { get; set; }

        public AddressModel Address { get; set; }

        public virtual string CompanyNumber { get; set; }
        public virtual DateTime Modified { get; set; } = VirtualDateTime.Now;

        #endregion

        #region Group Submission
        public bool GroupSubmission { get; set; }
        public virtual string ParentName { get; set; }
        public virtual long? ChildOrganisationId { get; set; }
        public virtual long? ChildStatementOrganisationId { get; set; }
        #endregion

        #region Your Statement
        public string StatementUrl { get; set; }
        public DateTime? StatementStartDate { get; set; }
        public DateTime? StatementEndDate { get; set; }
        public string ApprovingPerson { get; set; }
        public DateTime? ApprovedDate { get; set; }
        #endregion

        #region Compliance
        public bool? IncludesStructure { get; set; }
        public string StructureDetails { get; set; }

        public bool? IncludesPolicies { get; set; }
        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }
        public string RisksDetails { get; set; }

        public bool? IncludesDueDiligence { get; set; }
        public string DueDiligenceDetails { get; set; }

        public bool? IncludesTraining { get; set; }
        public string TrainingDetails { get; set; }

        public bool? IncludesGoals { get; set; }
        public string GoalsDetails { get; set; }
        #endregion

        #region Your Organisation
        public virtual int[] SectorTypeIds { get; set; }
        public List<string> Sectors { get; set; } = new List<string>();
        public string OtherSector { get; set; }

        public virtual int? Turnover { get; set; }
        #endregion

        #region Policies
        public List<string> Policies { get; set; } = new List<string>();
        public string OtherPolicies { get; set; }

        #endregion

        #region Supply Chain Risks
        public List<string> RelevantRisks { get; set; } = new List<string>();
        public string OtherRelevantRisks { get; set; }
        public List<string> HighRisks { get; set; } = new List<string>();
        public string OtherHighRisks { get; set; }

        public List<string> LocationRisks { get; set; } = new List<string>();
        #endregion

        #region Due Diligence
        public List<string> DueDiligences { get; set; } = new List<string>();
        public string ForcedLabourDetails { get; set; }
        public string SlaveryInstanceDetails { get; set; }
        public List<string> RemediationTypes { get; set; } = new List<string>();
        #endregion

        #region Training
        public List<string> Training { get; set; } = new List<string>();
        public string OtherTraining { get; set; }
        #endregion

        #region Monitoring progress
        public bool? IncludesMeasuringProgress { get; set; }
        public string ProgressMeasures { get; set; }
        public string KeyAchievements { get; set; }
        public int? StatementYears { get; set; }
        #endregion

        public OrganisationSearchModel SetSearchDocumentKey()
        {
            var key = ParentOrganisationId.ToString();
            if (SubmissionDeadlineYear.HasValue) key += $"-{SubmissionDeadlineYear}";
            if (!string.IsNullOrWhiteSpace(ParentName))
            {
                if (ChildOrganisationId.HasValue)
                    key += $"-{ChildOrganisationId}";
                else
                    key += $"-{OrganisationName.ToLower()}";
            }
            SearchDocumentKey = key;
            return this;
        }
    }
}