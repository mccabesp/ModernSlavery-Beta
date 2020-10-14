using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class YourStatementPageViewModelMapperProfile : Profile
    {
        public YourStatementPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, YourStatementPageViewModel>()
                .ForMember(d => d.StatementStartDay, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Day))
                .ForMember(d => d.StatementStartMonth, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Month))
                .ForMember(d => d.StatementStartYear, opt => opt.MapFrom(s => s.StatementStartDate == null ? (int?)null : s.StatementStartDate.Value.Year))
                .ForMember(d => d.StatementEndDay, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Day))
                .ForMember(d => d.StatementEndMonth, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Month))
                .ForMember(d => d.StatementEndYear, opt => opt.MapFrom(s => s.StatementEndDate == null ? (int?)null : s.StatementEndDate.Value.Year))
                .ForMember(d => d.ApprovedDay, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Day))
                .ForMember(d => d.ApprovedMonth, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Month))
                .ForMember(d => d.ApprovedYear, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Year));

            CreateMap<YourStatementPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(s => s.OrganisationId, opt => opt.Ignore())
                .ForMember(s => s.GroupSubmission, opt => opt.Ignore())
                .ForMember(d => d.StatementStartDate, opt => { opt.MapFrom(s => s.StatementStartDate); })
                .ForMember(d => d.StatementEndDate, opt => { opt.AllowNull(); opt.MapFrom(s => s.StatementEndDate); })
                .ForMember(d => d.ApprovedDate, opt => { opt.AllowNull(); opt.MapFrom(s => s.ApprovedDate); })
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.StatementStartYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementStartMonth, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementStartDay, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementEndYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementEndMonth, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.StatementEndDay, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedMonth, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedDay, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MinStartYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MaxStartYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MinEndYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MaxEndYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MinApprovedYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MaxApprovedYear, opt => opt.DoNotValidate());
        }
    }

    public class YourStatementPageViewModel : BaseViewModel
    {
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
        [JsonIgnore]
        [IgnoreMap]
        public int MinApprovedYear => ReportingDeadlineYear - 1;
        [JsonIgnore]
        [IgnoreMap]
        public int MaxApprovedYear => ReportingDeadlineYear;

        public override string PageTitle => "Your modern slavery statement";

        [BindNever]
        public bool? GroupSubmission { get; set; }

        [Url]
        [MaxLength(256)]
        public string StatementUrl { get; set; }

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

        [MaxLength(50)]
        public string ApproverJobTitle { get; set; }
        [MaxLength(50)]
        public string ApproverFirstName { get; set; }
        [MaxLength(50)]
        public string ApproverLastName { get; set; }

        public DateTime? ApprovedDate => ToDateTime(ApprovedYear, ApprovedMonth, ApprovedDay);

        [Range(1, 31)]
        public int? ApprovedDay { get; set; }
        [Range(1, 12)]
        public int? ApprovedMonth { get; set; }
        public int? ApprovedYear { get; set; }

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
                    validationResults.AddValidationError(3101, nameof(StatementStartDay));

                if (StatementStartMonth == null)
                    validationResults.AddValidationError(3102, nameof(StatementStartMonth));

                if (StatementStartYear == null)
                    validationResults.AddValidationError(3103, nameof(StatementStartYear));
            }

            // Must be a real date
            if (partsComplete && !StatementStartDate.HasValue)
                validationResults.AddValidationError(3129);

            // Cannot be later than today's date
            if (StatementStartDate.HasValue && StatementStartDate.Value > VirtualDateTime.Now)
                validationResults.AddValidationError(0);

            // Must be within the allowed years
            if (StatementStartYear != null && (StatementStartYear.Value > MaxStartYear || StatementStartYear.Value < MinStartYear))
                validationResults.AddValidationError(3119, nameof(StatementStartYear), new { minYear = MinStartYear, maxYear = MaxStartYear });

            #endregion

            #region End date validation

            //Validate the end date parts
            partsComplete = !Text.IsAnyNull(StatementEndDay, StatementEndMonth, StatementEndYear);
            partsEmpty = Text.IsAllNull(StatementEndDay, StatementEndMonth, StatementEndYear);
            if (!partsComplete && !partsEmpty)
            {
                if (StatementEndDay == null)
                    validationResults.AddValidationError(3105, nameof(StatementEndDay));

                if (StatementEndMonth == null)
                    validationResults.AddValidationError(3106, nameof(StatementEndMonth));

                if (StatementEndYear == null)
                    validationResults.AddValidationError(3107, nameof(StatementEndYear));
            }

            // Must be a real date
            if (partsComplete && !StatementEndDate.HasValue)
                validationResults.AddValidationError(3130);

            // Cannot be later than today's date
            if (StatementEndDate.HasValue && StatementEndDate.Value > VirtualDateTime.Now)
                validationResults.AddValidationError(0);

            // Must be within the allowed years
            if (StatementEndYear != null && (StatementEndYear.Value > MaxEndYear || StatementEndYear.Value < MinEndYear))
                validationResults.AddValidationError(3122, nameof(StatementEndYear), new { minYear = MinEndYear, maxYear = MaxEndYear });

            #endregion

            #region Start/End range validation

            // Validate the range when both start and end have real dates
            if (StatementStartDate.HasValue && StatementEndDate.HasValue)
            {
                // Can not start before it has finished
                if (StatementStartDate.Value >= StatementEndDate.Value)
                    validationResults.AddValidationError(0);

                //The period between from and to dates must be a minimum of 12 months and a max of 24 months
                var monthsDiff = ((StatementEndDate.Value.Year - StatementStartDate.Value.Year) * 12)
                    + StatementEndDate.Value.Month - StatementStartDate.Value.Month;
                if (monthsDiff < 12 || monthsDiff < 24)
                    validationResults.AddValidationError(0);
            }

            #endregion

            #region Approved date validation

            //Validate the approved date parts
            partsComplete = !Text.IsAnyNull(ApprovedDay, ApprovedMonth, ApprovedYear);
            partsEmpty = Text.IsAllNull(ApprovedDay, ApprovedMonth, ApprovedYear);
            if (!partsComplete && !partsEmpty)
            {
                if (ApprovedDay == null)
                    validationResults.AddValidationError(3109, nameof(ApprovedDay));

                if (ApprovedMonth == null)
                    validationResults.AddValidationError(3110, nameof(ApprovedMonth));

                if (ApprovedYear == null)
                    validationResults.AddValidationError(3111, nameof(ApprovedYear));
            }

            // Must be a real date
            if (partsComplete && !ApprovedDate.HasValue)
                validationResults.AddValidationError(3130);

            // Cannot be later than today's date
            if (ApprovedDate.HasValue && ApprovedDate.Value > VirtualDateTime.Now)
                validationResults.AddValidationError(0);

            // Cannot be before the statment end date
            if (ApprovedDate.HasValue && StatementEndDate.HasValue && ApprovedDate.Value < StatementEndDate.Value)
                validationResults.AddValidationError(0);

            // Must be within the allowed years
            if (ApprovedYear != null && (ApprovedYear.Value > MaxApprovedYear || ApprovedYear.Value < MinApprovedYear))
                validationResults.AddValidationError(3125, nameof(ApprovedYear), new { minYear = MinApprovedYear, maxYear = MaxApprovedYear });

            #endregion

            #region Approver validation

            //Validate the approver parts
            partsComplete = !Text.IsAnyNullOrWhiteSpace(ApproverFirstName, ApproverLastName, ApproverJobTitle);
            partsEmpty = Text.IsAllNullOrWhiteSpace(ApproverFirstName, ApproverLastName, ApproverJobTitle);
            if (!partsComplete && !partsEmpty)
            {
                if (string.IsNullOrWhiteSpace(ApproverFirstName))
                    validationResults.AddValidationError(3113, nameof(ApproverFirstName));

                if (string.IsNullOrWhiteSpace(ApproverLastName))
                    validationResults.AddValidationError(3114, nameof(ApproverLastName));

                if (string.IsNullOrWhiteSpace(ApproverJobTitle))
                    validationResults.AddValidationError(3115, nameof(ApproverJobTitle));
            }

            #endregion

            return validationResults;
        }

        public override bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(StatementUrl)
                && StatementStartDay.HasValue
                && StatementStartMonth.HasValue
                && StatementStartYear.HasValue
                && StatementEndDay.HasValue
                && StatementEndMonth.HasValue
                && StatementEndYear.HasValue
                && !string.IsNullOrWhiteSpace(ApproverFirstName)
                && !string.IsNullOrWhiteSpace(ApproverLastName)
                && !string.IsNullOrWhiteSpace(ApproverJobTitle)
                && ApprovedDay.HasValue
                && ApprovedMonth.HasValue
                && ApprovedYear.HasValue;
        }
    }
}