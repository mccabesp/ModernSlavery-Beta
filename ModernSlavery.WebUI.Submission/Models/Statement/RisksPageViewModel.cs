using AutoMapper;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
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
            CreateMap<StatementModel, RisksPageViewModel>();
            CreateMap<RisksPageViewModel, StatementModel>(MemberList.Source);
        }
    }

    public class RisksPageViewModel : BaseViewModel
    {
        public List<RiskViewModel> RelevantRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherRelevantRisks;

        public List<RiskViewModel> HighRisks { get; set; }
        [Display(Name = " If you want to specify an area not mentioned above, please provide details")]
        public string OtherHighRisks;

        public List<RiskViewModel> LocationRisks { get; set; }

        public class RiskViewModel
        {
            public short Id { get; set; }
            public short? ParentId { get; set; }
            public string Description { get; set; }
            public bool IsSelected { get; set; }
            //TODO: need to set this on presenter
            public string Category { get; set; }
            //TODO: need to set this on presenter
            public List<RiskViewModel> ChildRisks { get; set; }
            //TODO: need to set this on presenter
            // [MaxLength(100, ErrorMessage = "Reason can only be 100 characters or less")] - not possible as different for each block - handled on ui
            public string Details { get; set; }

        }

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
                if (item.IsSelected && item.Details == null)
                    validationList.Add(new ValidationResult($"Please explain why {item.Description} is one of your highest risks"));
            }

            return validationList;
        }
    }
}
