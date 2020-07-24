using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class RisksPageViewModelMapperProfile : Profile
    {
        public RisksPageViewModelMapperProfile()
        {
            CreateMap<StatementModel.RisksModel, RisksPageViewModel.RiskViewModel>().ReverseMap();

            CreateMap<StatementModel, RisksPageViewModel>()
                .ForMember(s => s.BackUrl, opt => opt.Ignore())
                .ForMember(s => s.CancelUrl, opt => opt.Ignore())
                .ForMember(s => s.ContinueUrl, opt => opt.Ignore());

            CreateMap<RisksPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.RelevantRisks, opt => opt.MapFrom(s=>s.RelevantRisks.Where(r=>r.Id>0)))
                .ForMember(d => d.HighRisks, opt => opt.MapFrom(s=>s.HighRisks.Where(r=>r.Id>0)))
                .ForMember(d => d.LocationRisks, opt => opt.MapFrom(s=>s.LocationRisks.Where(r=>r.Id>0)))
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
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

    public class RisksPageViewModel : BaseViewModel
    {
        #region Types
        public class RiskViewModel
        {
            public short Id { get; set; }
            public string Details { get; set; }
        }

        #endregion

        public override string PageTitle => "Supply chain risks and due diligence";
        public override string SubTitle => "Part 1";

        public List<RiskViewModel> RelevantRisks { get; set; }
        public string OtherRelevantRisks;

        public List<RiskViewModel> HighRisks { get; set; }
        public string OtherHighRisks;

        public List<RiskViewModel> LocationRisks { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Get the risk types
            var riskTypes=validationContext.GetService<RiskTypeIndex>();

            //TODO: clarify this with Sam G as comment doesnt match?? Select all that apply
            if (RelevantRisks.Count > 3)
                yield return new ValidationResult("Please select no more than 3 categories");

            //TODO: better way to identify this particular options
            var vulnerableGroupId = riskTypes.Single(x => x.Description.Equals("Other vulnerable groups")).Id;
            var otherVulnerableGroup = RelevantRisks.FirstOrDefault(x => x.Id == vulnerableGroupId);
            if (otherVulnerableGroup != null && string.IsNullOrWhiteSpace(otherVulnerableGroup.Details))
                yield return new ValidationResult("Please enter the other vulnerable group");

            var typeOfWorkId = riskTypes.Single(x => x.Description.Equals("Other type of work")).Id;
            var othertypeOfWork = RelevantRisks.FirstOrDefault(x => x.Id == typeOfWorkId);
            if (othertypeOfWork != null && string.IsNullOrWhiteSpace(othertypeOfWork.Details))
                yield return new ValidationResult("Please enter the other type of work");

            var sectorId = riskTypes.Single(x => x.Description.Equals("Other sector")).Id;
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
                yield return new ValidationResult($"Please explain why {riskTypes.Single(r=>r.Id==risk.Id).Description} is one of your highest risks");
        }

        public override bool IsComplete()
        {
            var vulnerableGroup = RelevantRisks.Single(x => x.Description.Equals("Other vulnerable groups"));
            var typeOfWork = RelevantRisks.Single(x => x.Description.Equals("Other type of work"));
            var sector = RelevantRisks.Single(x => x.Description.Equals("Other sector"));
            var vulnerableGroupHighRisk = HighRisks.Single(x => x.Description.Equals("Other vulnerable groups"));
            var typeOfWorkHighDetails = HighRisks.Single(x => x.Description.Equals("Other type of work"));
            var sectorHighDetails = HighRisks.Single(x => x.Description.Equals("Other sector"));

            return RelevantRisks.Any(x => x.IsSelected)
                && HighRisks.Any(x => x.IsSelected)
                && LocationRisks.Any(x => x.IsSelected)
                && !vulnerableGroup.IsSelected || !vulnerableGroup.Details.IsNullOrWhiteSpace()
                && !typeOfWork.IsSelected || !typeOfWork.Details.IsNullOrWhiteSpace()
                && !sector.IsSelected || !sector.Details.IsNullOrWhiteSpace()
                && !vulnerableGroup.IsSelected || !vulnerableGroup.Details.IsNullOrWhiteSpace()
                && !typeOfWorkHighDetails.IsSelected || !typeOfWorkHighDetails.Details.IsNullOrWhiteSpace()
                && !sectorHighDetails.IsSelected || !sectorHighDetails.Details.IsNullOrWhiteSpace()
                && HighRisks.All(x => !x.IsSelected || !x.Details.IsNullOrWhiteSpace());
        }
    }
}
