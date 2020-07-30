using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class RisksPageViewModelMapperProfile : Profile
    {
        public RisksPageViewModelMapperProfile()
        {
            CreateMap<StatementModel.RisksModel, SupplyChainRisksPageViewModel.RiskViewModel>().ReverseMap();

            CreateMap<StatementModel, SupplyChainRisksPageViewModel>();

            CreateMap<SupplyChainRisksPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.RelevantRisks, opt => opt.MapFrom(s=>s.RelevantRisks.Where(r=>r.Id>0)))
                .ForMember(d => d.HighRisks, opt => opt.MapFrom(s=>s.HighRisks.Where(r=>r.Id>0)))
                .ForMember(d => d.LocationRisks, opt => opt.MapFrom(s=>s.LocationRisks.Where(r=>r.Id>0)))
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.RiskTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.RelevantRisks, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.HighRisks, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.LocationRisks, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    [DependencyModelBinder]
    public class SupplyChainRisksPageViewModel : BaseViewModel
    {
        [IgnoreMap]
        [Newtonsoft.Json.JsonIgnore]//This needs to be Newtonsoft.Json.JsonIgnore namespace not System.Text.Json.Serialization.JsonIgnore
        public RiskTypeIndex RiskTypes { get; }
        public SupplyChainRisksPageViewModel(RiskTypeIndex riskTypes)
        {
            RiskTypes = riskTypes;
        }

        public SupplyChainRisksPageViewModel()
        {

        }

        #region Types
        public class RiskViewModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
        }

        #endregion

        public override string PageTitle => "Supply chain risks and due diligence";
        public override string SubTitle => "Part 1";

        public List<RiskViewModel> RelevantRisks { get; set; } = new List<RiskViewModel>();

        [MaxLength(1024)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherRelevantRisks;

        public List<RiskViewModel> HighRisks { get; set; } = new List<RiskViewModel>();
        
        [MaxLength(1024)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherHighRisks;

        public List<RiskViewModel> LocationRisks { get; set; } = new List<RiskViewModel>();

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (RelevantRisks.Count(r => r.Id > 0) > 3)
                validationResults.AddValidationError(3501, nameof(RelevantRisks));

            var vulnerableGroupId = RiskTypes.Single(x => x.Description.Equals("Other vulnerable groups")).Id;
            var vulnerableGroupIndex = RelevantRisks.FindIndex(r => r.Id == vulnerableGroupId);
            var otherVulnerableGroup = RelevantRisks.FirstOrDefault(x => x.Id == vulnerableGroupId);
            if (otherVulnerableGroup != null && string.IsNullOrWhiteSpace(otherVulnerableGroup.Details))
                validationResults.AddValidationError(3500, $"RelevantRisks[{vulnerableGroupIndex}].Details");

            var typeOfWorkId = RiskTypes.Single(x => x.Description.Equals("Other type of work")).Id;
            var typeOfWorkIndex = RelevantRisks.FindIndex(r => r.Id == typeOfWorkId);
            var othertypeOfWork = RelevantRisks.FirstOrDefault(x => x.Id == typeOfWorkId);
            if (othertypeOfWork != null && string.IsNullOrWhiteSpace(othertypeOfWork.Details))
                validationResults.AddValidationError(3500, $"RelevantRisks[{typeOfWorkIndex}].Details");

            var sectorId = RiskTypes.Single(x => x.Description.Equals("Other sector")).Id;
            var sectorIndex = RelevantRisks.FindIndex(r => r.Id == sectorId);
            var othersector = RelevantRisks.FirstOrDefault(x => x.Id == sectorId);
            if (othersector != null && string.IsNullOrWhiteSpace(othersector.Details))
                validationResults.AddValidationError(3500, $"RelevantRisks[{sectorIndex}].Details");

            if (HighRisks.Count(r=>r.Id>0) > 3)validationResults.AddValidationError(3501, nameof(HighRisks));

            for (var highRiskIndex=0; highRiskIndex<HighRisks.Count; highRiskIndex++)
            {
                if (HighRisks[highRiskIndex].Id > 0 && string.IsNullOrWhiteSpace(HighRisks[highRiskIndex].Details))
                    validationResults.AddValidationError(3500, $"HighRisks[{highRiskIndex}].Details");
            }

            //Remove all the empty risks
            RelevantRisks.RemoveAll(r => r.Id == 0);
            HighRisks.RemoveAll(r => r.Id == 0);
            LocationRisks.RemoveAll(r => r.Id == 0);

            return validationResults;
        }

        public override bool IsComplete()
        {
            var vulnerableGroup = RiskTypes.Single(x => x.Description.Equals("Other vulnerable groups"));
            var typeOfWork = RiskTypes.Single(x => x.Description.Equals("Other type of work"));
            var sector = RiskTypes.Single(x => x.Description.Equals("Other sector"));

            return RelevantRisks.Any()
                && HighRisks.Any()
                && LocationRisks.Any()
                && !RelevantRisks.Any(r=>r.Id==vulnerableGroup.Id && string.IsNullOrWhiteSpace(r.Details))
                && !RelevantRisks.Any(r => r.Id == typeOfWork.Id && string.IsNullOrWhiteSpace(r.Details))
                && !RelevantRisks.Any(r => r.Id == sector.Id && string.IsNullOrWhiteSpace(r.Details))
                && !HighRisks.Any(r => r.Id == vulnerableGroup.Id && string.IsNullOrWhiteSpace(r.Details))
                && !HighRisks.Any(r => r.Id == typeOfWork.Id && string.IsNullOrWhiteSpace(r.Details))
                && !HighRisks.Any(r => r.Id == sector.Id && string.IsNullOrWhiteSpace(r.Details));
        }
    }
}
