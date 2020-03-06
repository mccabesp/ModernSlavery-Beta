using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using ModernSlavery.Entities;
using ModernSlavery.Extensions;

namespace ModernSlavery.Infrastructure
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

        public static EmployerSearchModel Create(Organisation org, bool keyOnly = false, List<SicCodeSearchModel> listOfSicCodeSearchModels = null)
        {
            if (keyOnly)
            {
                return new EmployerSearchModel { OrganisationId = org.OrganisationId.ToString() };
            }

            // Get the last two names for the org. Most recent name first
            string[] names = org.OrganisationNames.Select(n => n.Name).Reverse().Take(2).ToArray();

            var abbreviations = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            names.ForEach(n => abbreviations.Add(n.ToAbbr()));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".")));
            var excludes = new[] { "Ltd", "Limited", "PLC", "Corporation", "Incorporated", "LLP", "The", "And", "&", "For", "Of", "To" };
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
            IEnumerable<OrganisationSicCode> sicCodes = org.GetSicCodes();

            Return[] submittedReports = org.GetSubmittedReports().ToArray();

            var result = new EmployerSearchModel
            {
                OrganisationId = org.OrganisationId.ToString(),
                OrganisationIdEncrypted = org.GetEncryptedId(),
                Name = org.OrganisationName,
                PreviousName = prevOrganisationName,
                PartialNameForSuffixSearches = org.OrganisationName,
                PartialNameForCompleteTokenSearches = org.OrganisationName,
                Abbreviations = abbreviations.ToArray(),
                Size = org.LatestReturn == null ? 0 : (int)org.LatestReturn.OrganisationSize,
                SicSectionIds = sicCodes.Select(sic => sic.SicCode.SicSectionId.ToString()).Distinct().ToArray(),
                SicSectionNames = sicCodes.Select(sic => sic.SicCode.SicSection.Description).Distinct().ToArray(),
                SicCodeIds = sicCodes.Select(sicCode => sicCode.SicCodeId.ToString()).Distinct().ToArray(),
                Address = org.LatestAddress?.GetAddressString(),
                LatestReportedDate = submittedReports.Select(x => x.Created).FirstOrDefault(),
                ReportedYears = submittedReports.Select(x => x.AccountingDate.Year.ToString()).ToArray(),
                ReportedLateYears =
                    submittedReports.Where(x => x.IsLateSubmission).Select(x => x.AccountingDate.Year.ToString()).ToArray(),
                ReportedExplanationYears = submittedReports.Where(x => string.IsNullOrEmpty(x.CompanyLinkToGPGInfo) == false)
                    .Select(x => x.AccountingDate.Year.ToString())
                    .ToArray()
            };

            if (listOfSicCodeSearchModels != null)
            {
                result.SicCodeListOfSynonyms = result.GetListOfSynonyms(result.SicCodeIds, listOfSicCodeSearchModels);
            }

            return result;
        }

        private string[] GetListOfSynonyms(string[] resultSicCodeIds, List<SicCodeSearchModel> listOfSicCodeSearchModels)
        {
            var result = new List<string>();

            foreach (string resultSicCodeId in resultSicCodeIds)
            {
                SicCodeSearchModel sicCodeSearchModel = listOfSicCodeSearchModels.FirstOrDefault(x => x.SicCodeId == resultSicCodeId);

                if (sicCodeSearchModel == null)
                {
                    continue;
                }

                result.Add(sicCodeSearchModel.SicCodeDescription);

                if (sicCodeSearchModel.SicCodeListOfSynonyms != null && sicCodeSearchModel.SicCodeListOfSynonyms.Length > 0)
                {
                    result.AddRange(sicCodeSearchModel.SicCodeListOfSynonyms);
                }
            }

            return result.Any()
                ? result.ToArray()
                : null;
        }

        #region Organisation Properties

        [Key]

        public string OrganisationId { get; set; }

        public string OrganisationIdEncrypted { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        [IsFilterable]
        [IsSortable]
        public string Name { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public string PreviousName { get; set; }

        [IsSearchable]
        public string PartialNameForSuffixSearches { get; set; }

        [IsSearchable]
        public string PartialNameForCompleteTokenSearches { get; set; }

        [IsSearchable]
        [Analyzer(AnalyzerName.AsString.EnLucene)]
        public string[] Abbreviations { get; set; }

        [IsFilterable]
        [IsSortable]
        [IsFacetable]
        public int Size { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] SicSectionIds { get; set; }

        public string[] SicSectionNames { get; set; }

        [IsSearchable]
        [IsFilterable]
        [IsFacetable]
        public string[] SicCodeIds { get; set; }

        [IsSearchable]
        public string[] SicCodeListOfSynonyms { get; set; }

        public string Address { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] ReportedYears { get; set; }

        [IsFilterable]
        [IsFacetable]
        public DateTimeOffset LatestReportedDate { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] ReportedLateYears { get; set; }

        [IsFilterable]
        [IsFacetable]
        public string[] ReportedExplanationYears { get; set; }

        #endregion

    }
}
