﻿using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PeriodCoveredViewModelMapperProfile : Profile
    {
        public PeriodCoveredViewModelMapperProfile()
        {
            CreateMap<StatementModel, PeriodCoveredViewModel>()
                .ForMember(d => d.StatementStartDay, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Day))
                .ForMember(d => d.StatementStartMonth, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Month))
                .ForMember(d => d.StatementStartYear, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Year))
                .ForMember(d => d.StatementEndDay, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Day))
                .ForMember(d => d.StatementEndMonth, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Month))
                .ForMember(d => d.StatementEndYear, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Year));

            CreateMap<PeriodCoveredViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.StatementStartDate, opt => { opt.MapFrom(s => s.StatementStartDate); })
                .ForMember(d => d.StatementEndDate, opt => { opt.AllowNull(); opt.MapFrom(s => s.StatementEndDate); });
        }
    }

    public class PeriodCoveredViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "What period does this statement cover?";

        [JsonIgnore]
        [IgnoreMap]
        public int MinStartYear => ReportingDeadlineYear - 2;
        [JsonIgnore]
        [IgnoreMap]
        public int MaxStartYear => ReportingDeadlineYear;
        [JsonIgnore]
        [IgnoreMap]
        public int MinEndYear => ReportingDeadlineYear - 1;
        [JsonIgnore]
        [IgnoreMap]
        public int MaxEndYear => ReportingDeadlineYear;

        private DateTime? ToDateTime(int? year, int? month, int? day)
        {
            if (year == null || month == null || day == null) return null;

            if (DateTime.TryParse($"{year}-{month}-{day}", out var result))
                return result;

            return null;
        }

        public DateTime? StatementStartDate => ToDateTime(StatementStartYear, StatementStartMonth, StatementStartDay);

        [Range(1, 31)]
        public int? StatementStartDay { get; set; }
        [Range(1, 12)]
        public int? StatementStartMonth { get; set; }

        public int? StatementStartYear { get; set; }

        public DateTime? StatementEndDate => ToDateTime(StatementEndYear, StatementEndMonth, StatementEndDay);

        [Range(1, 31)]
        public int? StatementEndDay { get; set; }
        [Range(1, 12)]
        public int? StatementEndMonth { get; set; }
        public int? StatementEndYear { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            #region Start date validation

            // Validate the start date parts
            var partsComplete = !Text.IsAnyNull(StatementStartDay, StatementStartMonth, StatementStartYear);
            var partsEmpty = Text.IsAllNull(StatementStartDay, StatementStartMonth, StatementStartYear);
            if (!partsComplete && !partsEmpty)
            {
                if (StatementStartDay == null)
                    validationResults.AddValidationError(3200, nameof(StatementStartDay));

                if (StatementStartMonth == null)
                    validationResults.AddValidationError(3202, nameof(StatementStartMonth));

                if (StatementStartYear == null)
                    validationResults.AddValidationError(3204, nameof(StatementStartYear));
            }

            // Must be a real date
            if (partsComplete && !StatementStartDate.HasValue)
                validationResults.AddValidationError(3211, nameof(StatementStartDate));

            // Cannot be later than today's date
            if (StatementStartDate.HasValue && StatementStartDate.Value > VirtualDateTime.Now)
                validationResults.AddValidationError(3213, nameof(StatementStartDate));

            // Must be within the allowed years
            if (StatementStartYear != null && (StatementStartYear.Value > MaxStartYear || StatementStartYear.Value < MinStartYear))
                validationResults.AddValidationError(3205, nameof(StatementStartYear), new { minYear = MinStartYear, maxYear = MaxStartYear });

            #endregion

            #region End date validation

            //Validate the end date parts
            partsComplete = !Text.IsAnyNull(StatementEndDay, StatementEndMonth, StatementEndYear);
            partsEmpty = Text.IsAllNull(StatementEndDay, StatementEndMonth, StatementEndYear);
            if (!partsComplete && !partsEmpty)
            {
                if (StatementEndDay == null)
                    validationResults.AddValidationError(3206, nameof(StatementEndDay));

                if (StatementEndMonth == null)
                    validationResults.AddValidationError(3208, nameof(StatementEndMonth));

                if (StatementEndYear == null)
                    validationResults.AddValidationError(3210, nameof(StatementEndYear));
            }

            // Must be a real date
            if (partsComplete && !StatementEndDate.HasValue)
                validationResults.AddValidationError(3212, nameof(StatementEndDate));

            // Cannot be later than today's date
            if (StatementEndDate.HasValue && StatementEndDate.Value > VirtualDateTime.Now)
                validationResults.AddValidationError(3214, nameof(StatementEndDate));

            // Must be within the allowed years
            if (StatementEndYear != null && (StatementEndYear.Value > MaxEndYear || StatementEndYear.Value < MinEndYear))
                validationResults.AddValidationError(3215, nameof(StatementEndYear), new { minYear = MinEndYear, maxYear = MaxEndYear });

            #endregion

            #region Start/End range validation

            // Validate the range when both start and end have real dates
            if (StatementStartDate.HasValue && StatementEndDate.HasValue)
            {
                // Can not start before it has finished
                if (StatementStartDate.Value >= StatementEndDate.Value)
                    validationResults.AddValidationError(3216, nameof(StatementStartDate));

                //The period between from and to dates must be a minimum of 12 months and a max of 18 months 
                var monthsDiff = ((StatementEndDate.Value.Year - StatementStartDate.Value.Year) * 12)
                    + StatementEndDate.Value.Month - StatementStartDate.Value.Month;

                // start is first day of the month and end is last day of the month
                // 1st Feb 2019 to 31st Jan 2020 fails incorrectly
                // 31st Jan 2019 to 30th Jan 2020 AND 2nd Feb 2019 to 1st Feb works correctly
                if (StatementStartDate.Value.AddDays(-1).Month != StatementStartDate.Value.Month
                    && StatementEndDate.Value.AddDays(1).Month != StatementEndDate.Value.Month)
                {
                    monthsDiff += 1;
                }
                if (monthsDiff < 12 || monthsDiff > 18)
                    validationResults.AddValidationError(3217, nameof(StatementStartDate));
            }

            #endregion





            return validationResults;
        }

        public override Status GetStatus()
        {
            if (StatementStartDay.HasValue
                && StatementStartMonth.HasValue
                && StatementStartYear.HasValue
                && StatementEndDay.HasValue
                && StatementEndMonth.HasValue
                && StatementEndYear.HasValue) return Status.Complete;

            else if (StatementStartDate.HasValue || StatementEndDate.HasValue) return Status.InProgress;

            return Status.Incomplete;
        }


    }
}