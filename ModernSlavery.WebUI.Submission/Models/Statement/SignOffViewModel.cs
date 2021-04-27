using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class SignOffViewModelMapperProfile : Profile
    {
        public SignOffViewModelMapperProfile()
        {
            CreateMap<StatementModel, SignOffViewModel>()
                .ForMember(d => d.ApproverFirstName, opt => opt.MapFrom(s => s.ApproverFirstName))
                .ForMember(d => d.ApproverLastName, opt => opt.MapFrom(s => s.ApproverLastName))
                .ForMember(d => d.ApproverJobTitle, opt => opt.MapFrom(s => s.ApproverJobTitle))
                .ForMember(d => d.ApprovedDay, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Day))
                .ForMember(d => d.ApprovedMonth, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Month))
                .ForMember(d => d.ApprovedYear, opt => opt.MapFrom(s => s.ApprovedDate == null ? (int?)null : s.ApprovedDate.Value.Year))
                .ForMember(d => d.StatementEndDate, opt => opt.MapFrom(s => s.StatementEndDate));

            CreateMap<SignOffViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(d => d.ApproverFirstName, opt => opt.MapFrom(s => s.ApproverFirstName))
                .ForMember(d => d.ApproverLastName, opt => opt.MapFrom(s => s.ApproverLastName))
                .ForMember(d => d.ApproverJobTitle, opt => opt.MapFrom(s => s.ApproverJobTitle))
                .ForMember(d => d.ApprovedDate, opt => { opt.AllowNull(); opt.MapFrom(s => s.ApprovedDate); });
        }
    }

    public class SignOffViewModel : BaseStatementViewModel
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
        [Text]
        public string ApproverJobTitle { get; set; }
        [MaxLength(50)]
        [Text]
        public string ApproverFirstName { get; set; }
        [MaxLength(50)]
        [Text]
        public string ApproverLastName { get; set; }

        public DateTime? ApprovedDate => ToDateTime(ApprovedYear, ApprovedMonth, ApprovedDay);

        [Range(1, 31)]
        public int? ApprovedDay { get; set; }
        [Range(1, 12)]
        public int? ApprovedMonth { get; set; }
        public int? ApprovedYear { get; set; }

        public DateTime? StatementEndDate { get; set; }

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
                    validationResults.AddValidationError(3300, nameof(ApprovedDay));

                if (ApprovedMonth == null)
                    validationResults.AddValidationError(3301, nameof(ApprovedMonth));

                if (ApprovedYear == null)
                    validationResults.AddValidationError(3302, nameof(ApprovedYear));
            }

            // Must be a real date
            if (partsComplete && !ApprovedDate.HasValue)
                validationResults.AddValidationError(3303, nameof(ApprovedDate));

            // Cannot be later than today's date
            if (ApprovedDate.HasValue && ApprovedDate.Value > VirtualDateTime.Now)
                validationResults.AddValidationError(3304, nameof(ApprovedDate));

            // Must be within the allowed years
            if (ApprovedYear != null && (ApprovedYear.Value > MaxApprovedYear || ApprovedYear.Value < MinApprovedYear))
                validationResults.AddValidationError(3305, nameof(ApprovedYear), new { minYear = MinApprovedYear, maxYear = MaxApprovedYear });

            // Cannot be before the statement end date
            if (ApprovedDate.HasValue && StatementEndDate.HasValue && ApprovedDate.Value < StatementEndDate.Value)
                validationResults.AddValidationError(3309, nameof(ApprovedDate), new { minDate = StatementEndDate.Value.ToShortDateString() });

            #endregion

            #region Approver validation

            //Validate the approver parts
            partsComplete = !Text.IsAnyNullOrWhiteSpace(ApproverFirstName, ApproverLastName, ApproverJobTitle);
            partsEmpty = Text.IsAllNullOrWhiteSpace(ApproverFirstName, ApproverLastName, ApproverJobTitle);
            if (!partsComplete && !partsEmpty)
            {
                if (string.IsNullOrWhiteSpace(ApproverFirstName))
                    validationResults.AddValidationError(3306, nameof(ApproverFirstName));

                if (string.IsNullOrWhiteSpace(ApproverLastName))
                    validationResults.AddValidationError(3307, nameof(ApproverLastName));

                if (string.IsNullOrWhiteSpace(ApproverJobTitle))
                    validationResults.AddValidationError(3308, nameof(ApproverJobTitle));
            }

            #endregion

            return validationResults;
        }

        public override Status GetStatus()
        {
            var approverDetailsComplete = !string.IsNullOrWhiteSpace(ApproverFirstName)
                && !string.IsNullOrWhiteSpace(ApproverLastName)
                && !string.IsNullOrWhiteSpace(ApproverJobTitle);

            var approverDateComplete = ApprovedDay.HasValue
                && ApprovedMonth.HasValue
                && ApprovedYear.HasValue;

            if (approverDetailsComplete && approverDateComplete) return Status.Complete;

            else if (approverDateComplete || approverDetailsComplete) return Status.InProgress;

            else return Status.Incomplete;
        }
    }
}