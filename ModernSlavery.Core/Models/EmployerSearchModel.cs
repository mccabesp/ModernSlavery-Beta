using System;
using System.Collections.Generic;
using System.Linq;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Models
{
    [Serializable]
    public class EmployerSearchModel
    {
        public bool Equals(EmployerSearchModel model)
        {
            return model != null && model.OrganisationId == OrganisationId;
        }

        public override bool Equals(object obj)
        {
            var target = obj as EmployerSearchModel;
            return target != null && target.OrganisationId == OrganisationId;
        }

        public override int GetHashCode()
        {
            return OrganisationId.GetHashCode();
        }

        #region Organisation Properties

        public virtual string OrganisationId { get; set; }
        public string OrganisationIdEncrypted { get; set; }
        public virtual string Name { get; set; }
        public virtual string PreviousName { get; set; }
        public virtual string PartialNameForSuffixSearches { get; set; }
        public string PartialNameForCompleteTokenSearches { get; set; }
        public virtual string[] Abbreviations { get; set; }
        public virtual int Size { get; set; }
        public virtual string[] SicSectionIds { get; set; }
        public virtual string[] SicSectionNames { get; set; }
        public virtual string[] SicCodeIds { get; set; }
        public virtual string[] SicCodeListOfSynonyms { get; set; }
        public string Address { get; set; }
        public virtual string[] ReportedYears { get; set; }
        public virtual DateTimeOffset LatestReportedDate { get; set; }
        public virtual string[] ReportedLateYears { get; set; }
        public virtual string[] ReportedExplanationYears { get; set; }

        #endregion
    }
}