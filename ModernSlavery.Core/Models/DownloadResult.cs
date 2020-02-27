using ModernSlavery.Entities;
using ModernSlavery.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.Core.Models
{
    public class DownloadResult
    {

        public string EmployerName { get; set; }
        public string Address { get; set; }
        public string CompanyNumber { get; set; }
        public string SicCodes { get; set; }
        public decimal DiffMeanHourlyPercent { get; set; }
        public decimal DiffMedianHourlyPercent { get; set; }
        public decimal? DiffMeanBonusPercent { get; set; }
        public decimal? DiffMedianBonusPercent { get; set; }
        public decimal MaleBonusPercent { get; set; }
        public decimal FemaleBonusPercent { get; set; }
        public decimal MaleLowerQuartile { get; set; }
        public decimal FemaleLowerQuartile { get; set; }
        public decimal MaleLowerMiddleQuartile { get; set; }
        public decimal FemaleLowerMiddleQuartile { get; set; }
        public decimal MaleUpperMiddleQuartile { get; set; }
        public decimal FemaleUpperMiddleQuartile { get; set; }
        public decimal MaleTopQuartile { get; set; }
        public decimal FemaleTopQuartile { get; set; }
        public string CompanyLinkToGPGInfo { get; set; }
        public string ResponsiblePerson { get; set; }
        public string EmployerSize { get; set; }
        public string CurrentName { get; set; }
        public bool SubmittedAfterTheDeadline { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime DateSubmitted { get; set; }

        public static DownloadResult Create(Return @return)
        {
            return new DownloadResult
            {
                EmployerName = @return.Organisation.GetName(@return.StatusDate)?.Name ?? @return.Organisation.OrganisationName,
                Address = @return.Organisation.GetAddressString(@return.StatusDate, delimiter: "," + Environment.NewLine),
                CompanyNumber = @return.Organisation?.CompanyNumber,
                SicCodes = @return.Organisation?.GetSicCodeIdsString(@return.StatusDate, "," + Environment.NewLine),
                DiffMeanHourlyPercent = @return.DiffMeanHourlyPayPercent,
                DiffMedianHourlyPercent = @return.DiffMedianHourlyPercent,
                DiffMeanBonusPercent = @return.DiffMeanBonusPercent,
                DiffMedianBonusPercent = @return.DiffMedianBonusPercent,
                MaleBonusPercent = @return.MaleMedianBonusPayPercent,
                FemaleBonusPercent = @return.FemaleMedianBonusPayPercent,
                MaleLowerQuartile = @return.MaleLowerPayBand,
                FemaleLowerQuartile = @return.FemaleLowerPayBand,
                MaleLowerMiddleQuartile = @return.MaleMiddlePayBand,
                FemaleLowerMiddleQuartile = @return.FemaleMiddlePayBand,
                MaleUpperMiddleQuartile = @return.MaleUpperPayBand,
                FemaleUpperMiddleQuartile = @return.FemaleUpperPayBand,
                MaleTopQuartile = @return.MaleUpperQuartilePayBand,
                FemaleTopQuartile = @return.FemaleUpperQuartilePayBand,
                CompanyLinkToGPGInfo = @return.CompanyLinkToGPGInfo,
                ResponsiblePerson = @return.ResponsiblePerson,
                EmployerSize = @return.OrganisationSize.GetAttribute<DisplayAttribute>().Name,
                CurrentName = @return.Organisation?.OrganisationName,
                SubmittedAfterTheDeadline = @return.IsLateSubmission,
                DueDate = @return.AccountingDate.AddYears(1),
                DateSubmitted = @return.Modified
            };
        }

    }
}
