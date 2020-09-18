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
        public override bool Equals(object obj)
        {
            var target = obj as OrganisationSearchModel;
            return target != null && target.SearchDocumentKey == SearchDocumentKey;
        }

        public override int GetHashCode()
        {
            return SearchDocumentKey.GetHashCode();
        }

        public virtual string SearchDocumentKey { get; set; }

        #region Organisation Properties
        public virtual long ParentOrganisationId { get; set; }
        public virtual long? ChildOrganisationId { get; set; }
        public virtual string CompanyNumber { get; set; }
        public virtual long? StatementId { get; set; }
        public virtual string OrganisationName { get; set; }
        public virtual string ParentName { get; set; }
        public virtual string PartialNameForSuffixSearches { get; set; }
        public virtual string PartialNameForCompleteTokenSearches { get; set; }
        public virtual string[] Abbreviations { get; set; }
        public virtual int? Turnover { get; set; }
        public virtual int[] SectorTypeIds { get; set; }

        public bool IsParent { get; set; }
        public virtual long? ChildStatementOrganisationId { get; set; }

        public string Address { get; set; }
        public virtual int? StatementDeadlineYear { get; set; }
        public virtual DateTime Modified { get; set; } = VirtualDateTime.Now;
        public virtual DateTime Timestamp { get; } = VirtualDateTime.Now;
        #endregion

        public OrganisationSearchModel SetSearchDocumentKey()
        {
            var key = ParentOrganisationId.ToString();
            if (StatementDeadlineYear.HasValue) key += $"-{StatementDeadlineYear}";
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