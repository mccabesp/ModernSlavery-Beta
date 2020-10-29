using AutoMapper;
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
    public class ApprovalPageViewModelMapperProfile : Profile
    {
        public ApprovalPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, ApprovalPageViewModel>()
                .ForMember(d => d.ApprovedDay, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Day))
                .ForMember(d => d.ApprovedMonth, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Month))
                .ForMember(d => d.ApprovedYear, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Year));

            CreateMap<ApprovalPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(s => s.OrganisationId, opt => opt.Ignore())
                .ForMember(s => s.GroupSubmission, opt => opt.Ignore())
                .ForMember(d => d.ApprovedDate, opt => { opt.AllowNull(); opt.MapFrom(s => s.ApprovedDate); })
                .ForSourceMember(s => s.ApprovedYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedMonth, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ApprovedDay, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MinApprovedYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.MaxApprovedYear, opt => opt.DoNotValidate());
        }
    }

    public class ApprovalPageViewModel : BaseViewModel
    {
        public override string PageTitle => "What is the name of the director (or equivalent) who signed off your statement?";

        public int MinApprovedYear => ReportingDeadlineYear - 1;
        [JsonIgnore]
        [IgnoreMap]
        public int MaxApprovedYear => ReportingDeadlineYear;

        private DateTime? ToDateTime(int? year, int? month, int? day)
        {
            if (year == null || month == null || day == null) return null;

            if (DateTime.TryParse($"{year}-{month}-{day}", out var result))
                return result;

            return null;
        }

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

            #region Approved date validation

            //Validate the approved date parts
            var partsComplete = !Text.IsAnyNull(ApprovedDay, ApprovedMonth, ApprovedYear);
            var partsEmpty = Text.IsAllNull(ApprovedDay, ApprovedMonth, ApprovedYear);
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
                validationResults.AddValidationError(3130, nameof(ApprovedDate));

            // Cannot be later than today's date
            if (ApprovedDate.HasValue && ApprovedDate.Value > VirtualDateTime.Now)
                validationResults.AddValidationError(3134, nameof(ApprovedDate));

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
            return !string.IsNullOrWhiteSpace(ApproverFirstName)
                && !string.IsNullOrWhiteSpace(ApproverLastName)
                && !string.IsNullOrWhiteSpace(ApproverJobTitle)
                && ApprovedDay.HasValue
                && ApprovedMonth.HasValue
                && ApprovedYear.HasValue;
        }
    }
}