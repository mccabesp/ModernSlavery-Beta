using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class OrganisationSearchModel
    {
        public bool Equals(OrganisationSearchModel model)
        {
            return model != null && model.OrganisationId == OrganisationId;
        }

        public override bool Equals(object obj)
        {
            var target = obj as OrganisationSearchModel;
            return target != null && target.SearchDocumentKey == SearchDocumentKey;
        }

        public override int GetHashCode()
        {
            return OrganisationId.GetHashCode();
        }

        public virtual string SearchDocumentKey { get; set; }

        #region Organisation Properties
        public virtual long OrganisationId { get; set; }
        public virtual long ChildOrganisationId { get; set; }
        public virtual string CompanyNumber { get; set; }
        public virtual long? StatementId { get; set; }
        public virtual string Name { get; set; }
        public virtual string PreviousName { get; set; }
        public virtual string PartialNameForSuffixSearches { get; set; }
        public virtual string PartialNameForCompleteTokenSearches { get; set; }
        public virtual string[] Abbreviations { get; set; }
        public virtual int? Turnover { get; set; }
        public virtual int[] SectorTypeIds { get; set; }

        public bool IsParent { get; set; }
        public virtual long? ChildStatementOrganisationId { get; set; }

        public string Address { get; set; }
        public virtual int? StatementDeadlineYear { get; set; }
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        #endregion

        public OrganisationSearchModel SetSearchDocumentKey()
        {
            var key = OrganisationId.ToString();
            if (StatementDeadlineYear.HasValue) key += $"-{StatementDeadlineYear}";
            if (ChildStatementOrganisationId.HasValue) key += $"-{ChildStatementOrganisationId}";
            SearchDocumentKey=key;
            return this;
        }
    }
}