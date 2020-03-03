using System;

namespace ModernSlavery.Entities
{
    public partial class OrganisationSubmissionInfoView
    {
        public long OrganisationId { get; set; }
        public DateTime? LatestReturnAccountingDate { get; set; }
        public DateTime? ReportingDeadline { get; set; }
        public string LatestReturnStatus { get; set; }
        public DateTime LatestReturnStatusDate { get; set; }
        public DateTime? DateFirstReportedInYear { get; set; }
        public string LatestReturnStatusDetails { get; set; }
        public string ReportedLate { get; set; }
        public string LatestReturnLateReason { get; set; }
        public string StatusId { get; set; }
        public DateTime? StatusDate { get; set; }
        public string StatusDetails { get; set; }
        public string ReturnModifiedFields { get; set; }
        public string Ehrcresponse { get; set; }
        public string SubmittedBy { get; set; }
        public string OrganisationSize { get; set; }
        public decimal DiffMeanHourlyPayPercent { get; set; }
        public decimal DiffMedianHourlyPercent { get; set; }
        public decimal? DiffMeanBonusPercent { get; set; }
        public decimal? DiffMedianBonusPercent { get; set; }
        public decimal MaleMedianBonusPayPercent { get; set; }
        public decimal FemaleMedianBonusPayPercent { get; set; }
        public decimal MaleLowerPayBand { get; set; }
        public decimal FemaleLowerPayBand { get; set; }
        public decimal MaleMiddlePayBand { get; set; }
        public decimal FemaleMiddlePayBand { get; set; }
        public decimal MaleUpperPayBand { get; set; }
        public decimal FemaleUpperPayBand { get; set; }
        public decimal MaleUpperQuartilePayBand { get; set; }
        public decimal FemaleUpperQuartilePayBand { get; set; }
        public string CompanyLinkToGpginfo { get; set; }
    }
}
