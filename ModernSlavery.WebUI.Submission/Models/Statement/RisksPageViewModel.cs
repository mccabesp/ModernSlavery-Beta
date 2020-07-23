using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Emit;
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
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
            public string Category { get; set; }
            public string Details { get; set; }

        }
        #endregion

        public override string PageTitle => "Supply chain risks and due diligence";
        public override string SubTitle => "Part 1";

        public List<RiskViewModel> RelevantRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherRelevantRisks;

        public List<RiskViewModel> HighRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherHighRisks;

        public List<RiskViewModel> LocationRisks { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            var validationList = new List<ValidationResult>();
            //TODO: clarify this with Sam G as comment doesnt match?? Select all that apply
            if (RelevantRisks.Count > 3)
                validationList.Add(new ValidationResult("Please select no more than 3 categories"));


            //TODO: better way to identify this particular options
            var vulnerableGroup = RelevantRisks.Single(x => x.Description.Equals("Other vulnerable groups"));
            if (vulnerableGroup.IsSelected && vulnerableGroup.Details.IsNullOrWhiteSpace())
                validationList.Add(new ValidationResult("Please enter the other vulnerable group"));

            var typeOfWork = RelevantRisks.Single(x => x.Description.Equals("Other type of work"));
            if (typeOfWork.IsSelected && typeOfWork.Details.IsNullOrWhiteSpace())
                validationList.Add(new ValidationResult("Please enter the other type of work"));

            var sector = RelevantRisks.Single(x => x.Description.Equals("Other sector"));
            if (sector.IsSelected && sector.Details.IsNullOrWhiteSpace())
                validationList.Add(new ValidationResult("Please enter the other sector"));

            if (HighRisks.Count > 3)
                validationList.Add(new ValidationResult("Please select no more than 3 high risk areas"));

            var vulnerableGroupHighRisk = HighRisks.Single(x => x.Description.Equals("Other vulnerable groups"));
            if (vulnerableGroupHighRisk.IsSelected && vulnerableGroupHighRisk.Details.IsNullOrWhiteSpace())
                validationList.Add(new ValidationResult("Please enter the other vulnerable group for high risk area"));

            var typeOfWorkHighDetails = HighRisks.Single(x => x.Description.Equals("Other type of work"));
            if (typeOfWorkHighDetails.IsSelected && typeOfWorkHighDetails.Details.IsNullOrWhiteSpace())
                validationList.Add(new ValidationResult("Please enter the other type of work for high rissk area"));

            var sectorHighDetails = HighRisks.Single(x => x.Description.Equals("Other sector"));
            if (sectorHighDetails.IsSelected && sectorHighDetails.Details.IsNullOrWhiteSpace())
                validationList.Add(new ValidationResult("Please enter the other sector for high risk area"));

            foreach (var item in HighRisks)
            {
                if (item.IsSelected && item.Details.IsNullOrWhiteSpace())
                    validationList.Add(new ValidationResult($"Please explain why {item.Description} is one of your highest risks"));
            }

            return validationList;
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
