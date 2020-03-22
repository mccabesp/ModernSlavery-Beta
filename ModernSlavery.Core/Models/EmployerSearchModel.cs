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

        public string GetEncryptedOrganisionId()
        {
            return Encryption.EncryptQuerystring(OrganisationId);
        }

        public static EmployerSearchModel Create(Organisation org, bool keyOnly = false,
            List<SicCodeSearchModel> listOfSicCodeSearchModels = null)
        {
            if (keyOnly) return new EmployerSearchModel {OrganisationId = org.OrganisationId.ToString()};

            // Get the last two names for the org. Most recent name first
            var names = org.OrganisationNames.Select(n => n.Name).Reverse().Take(2).ToArray();

            var abbreviations = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            names.ForEach(n => abbreviations.Add(n.ToAbbr()));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".")));
            var excludes = new[]
                {"Ltd", "Limited", "PLC", "Corporation", "Incorporated", "LLP", "The", "And", "&", "For", "Of", "To"};
            names.ForEach(n => abbreviations.Add(n.ToAbbr(excludeWords: excludes)));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".", excludeWords: excludes)));

            abbreviations.RemoveWhere(a => string.IsNullOrWhiteSpace(a));
            abbreviations.Remove(org.OrganisationName);

            // extract the prev org name (if exists)
            var prevOrganisationName = "";
            if (names.Length > 1)
            {
                prevOrganisationName = names[names.Length - 1];
                abbreviations.Remove(prevOrganisationName);
            }

            //Get the latest sic codes
            var sicCodes = org.GetSicCodes();

            var submittedReports = org.GetSubmittedReports().ToArray();

            var result = new EmployerSearchModel
            {
                OrganisationId = org.OrganisationId.ToString(),
                OrganisationIdEncrypted = org.GetEncryptedId(),
                Name = org.OrganisationName,
                PreviousName = prevOrganisationName,
                PartialNameForSuffixSearches = org.OrganisationName,
                PartialNameForCompleteTokenSearches = org.OrganisationName,
                Abbreviations = abbreviations.ToArray(),
                Size = org.LatestReturn == null ? 0 : (int) org.LatestReturn.OrganisationSize,
                SicSectionIds = sicCodes.Select(sic => sic.SicCode.SicSectionId.ToString()).Distinct().ToArray(),
                SicSectionNames = sicCodes.Select(sic => sic.SicCode.SicSection.Description).Distinct().ToArray(),
                SicCodeIds = sicCodes.Select(sicCode => sicCode.SicCodeId.ToString()).Distinct().ToArray(),
                Address = org.LatestAddress?.GetAddressString(),
                LatestReportedDate = submittedReports.Select(x => x.Created).FirstOrDefault(),
                ReportedYears = submittedReports.Select(x => x.AccountingDate.Year.ToString()).ToArray(),
                ReportedLateYears =
                    submittedReports.Where(x => x.IsLateSubmission).Select(x => x.AccountingDate.Year.ToString())
                        .ToArray(),
                ReportedExplanationYears = submittedReports
                    .Where(x => string.IsNullOrEmpty(x.CompanyLinkToGPGInfo) == false)
                    .Select(x => x.AccountingDate.Year.ToString())
                    .ToArray()
            };

            if (listOfSicCodeSearchModels != null)
                result.SicCodeListOfSynonyms = result.GetListOfSynonyms(result.SicCodeIds, listOfSicCodeSearchModels);

            return result;
        }

        private string[] GetListOfSynonyms(string[] resultSicCodeIds,
            List<SicCodeSearchModel> listOfSicCodeSearchModels)
        {
            var result = new List<string>();

            foreach (var resultSicCodeId in resultSicCodeIds)
            {
                var sicCodeSearchModel = listOfSicCodeSearchModels.FirstOrDefault(x => x.SicCodeId == resultSicCodeId);

                if (sicCodeSearchModel == null) continue;

                result.Add(sicCodeSearchModel.SicCodeDescription);

                if (sicCodeSearchModel.SicCodeListOfSynonyms != null &&
                    sicCodeSearchModel.SicCodeListOfSynonyms.Length > 0)
                    result.AddRange(sicCodeSearchModel.SicCodeListOfSynonyms);
            }

            return result.Any()
                ? result.ToArray()
                : null;
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