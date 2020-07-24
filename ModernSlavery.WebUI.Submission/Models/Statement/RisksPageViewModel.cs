using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class RisksPageViewModelMapperProfile : Profile
    {
        public RisksPageViewModelMapperProfile()
        {
            CreateMap<StatementModel.RisksModel, RisksPageViewModel.RiskViewModel>().ReverseMap();

            CreateMap<StatementModel, RisksPageViewModel>()
                .ForMember(s => s.RiskTypes, opt => opt.Ignore())
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<RisksPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.RiskTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    public class RisksPageViewModel : BaseViewModel
    {
        public RisksPageViewModel(RiskTypeIndex riskTypes)
        {
            RiskTypes = riskTypes;
        }

        #region Types
        public class RiskViewModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
        }

        #endregion

        public override string PageTitle => "Supply chain risks and due diligence";
        public override string SubTitle => "Part 2";

        public RiskTypeIndex RiskTypes { get; }

        public List<RiskViewModel> RelevantRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherRelevantRisks;

        public List<RiskViewModel> HighRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherHighRisks;

        public List<RiskViewModel> LocationRisks { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //TODO: clarify this with Sam G as comment doesnt match?? Select all that apply
            if (RelevantRisks.Count > 3)
                yield return new ValidationResult("Please select no more than 3 categories");

            //TODO: better way to identify this particular options
            var vulnerableGroupId = RiskTypes.Single(x => x.Description.Equals("Other vulnerable groups")).Id;
            var otherVulnerableGroup = RelevantRisks.FirstOrDefault(x => x.Id == vulnerableGroupId);
            if (otherVulnerableGroup != null && string.IsNullOrWhiteSpace(otherVulnerableGroup.Details))
                yield return new ValidationResult("Please enter the other vulnerable group");

            var typeOfWorkId = RiskTypes.Single(x => x.Description.Equals("Other type of work")).Id;
            var othertypeOfWork = RelevantRisks.FirstOrDefault(x => x.Id == typeOfWorkId);
            if (othertypeOfWork != null && string.IsNullOrWhiteSpace(othertypeOfWork.Details))
                yield return new ValidationResult("Please enter the other type of work");

            var sectorId = RiskTypes.Single(x => x.Description.Equals("Other sector")).Id;
            var othersector = RelevantRisks.FirstOrDefault(x => x.Id == sectorId);
            if (othersector != null && string.IsNullOrWhiteSpace(othersector.Details))
                yield return new ValidationResult("Please enter the other sector");

            if (HighRisks.Count > 3)
                yield return new ValidationResult("Please select no more than 3 high risk areas");

            otherVulnerableGroup = HighRisks.FirstOrDefault(x => x.Id == vulnerableGroupId);
            if (otherVulnerableGroup != null && string.IsNullOrWhiteSpace(otherVulnerableGroup.Details))
                yield return new ValidationResult("Please enter the other vulnerable group for high risk area");

            othertypeOfWork = HighRisks.FirstOrDefault(x => x.Id == typeOfWorkId);
            if (othertypeOfWork != null && string.IsNullOrWhiteSpace(othertypeOfWork.Details))
                yield return new ValidationResult("Please enter the other type of work for high risk area");

            othersector = HighRisks.FirstOrDefault(x => x.Id == sectorId);
            if (othersector != null && string.IsNullOrWhiteSpace(othersector.Details))
                yield return new ValidationResult("Please enter the other sector for high risk area");

            foreach (var risk in HighRisks.Where(r=>string.IsNullOrWhiteSpace(r.Details)))
                yield return new ValidationResult($"Please explain why {RiskTypes.Single(r=>r.Id==risk.Id).Description} is one of your highest risks");
        }

        public override bool IsComplete()
        {
            return base.IsComplete();
        }
    }
}
