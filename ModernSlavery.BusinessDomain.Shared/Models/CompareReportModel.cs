using System;
using System.ComponentModel.DataAnnotations;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using static ModernSlavery.BusinessDomain.Shared.Models.StatementModel;

namespace ModernSlavery.BusinessDomain.Shared.Models
{
    [Serializable]
    public sealed class CompareReportModel
    {
        public bool HasReported { get; set; }

        public string EncOrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public ScopeStatuses ScopeStatus { get; set; }

        public decimal? DiffMeanHourlyPayPercent { get; set; }

        public decimal? DiffMedianHourlyPercent { get; set; }

        public decimal? DiffMeanBonusPercent { get; set; }

        public decimal? DiffMedianBonusPercent { get; set; }

        public decimal? MaleMedianBonusPayPercent { get; set; }

        public decimal? FemaleMedianBonusPayPercent { get; set; }

        public decimal? FemaleLowerPayBand { get; set; }

        public decimal? FemaleMiddlePayBand { get; set; }

        public decimal? FemaleUpperPayBand { get; set; }

        public decimal? FemaleUpperQuartilePayBand { get; set; }

        public TurnoverRanges? TurnoverRange { get; set; }

        public bool? HasBonusesPaid { get; set; }

        public bool RequiredToReport =>
            ScopeStatus == ScopeStatuses.InScope || ScopeStatus == ScopeStatuses.PresumedInScope;

        public string TurnoverName =>
            HasReported
                ? TurnoverRange.GetAttribute<DisplayAttribute>().Name
                : TurnoverRanges.NotProvided.GetAttribute<DisplayAttribute>().Name;
    }
}