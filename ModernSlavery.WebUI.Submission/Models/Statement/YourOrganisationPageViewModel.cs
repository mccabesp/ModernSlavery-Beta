using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using System.Linq;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using ModernSlavery.WebUI.Shared.Classes.Binding;
using ModernSlavery.Core.Classes.StatementTypeIndexes;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class OrganisationPageViewModelMapperProfile : Profile
    {
        public OrganisationPageViewModelMapperProfile()
        {
            CreateMap<StatementModel, YourOrganisationPageViewModel>();

            CreateMap<YourOrganisationPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForMember(s => s.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.GroupSubmission, opt => opt.Ignore())
                .ForSourceMember(s => s.SectorTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    [DependencyModelBinder]
    public class YourOrganisationPageViewModel : BaseViewModel
    {
        [IgnoreMap]
        [Newtonsoft.Json.JsonIgnore]//This needs to be Newtonsoft.Json.JsonIgnore namespace not System.Text.Json.Serialization.JsonIgnore
        public SectorTypeIndex SectorTypes { get; }
        public YourOrganisationPageViewModel(SectorTypeIndex sectorTypes)
        {
            SectorTypes = sectorTypes;
        }

        public YourOrganisationPageViewModel()
        {

        }

        #region Types
        public enum TurnoverRanges : byte
        {
            //Not Provided
            NotProvided = 0,

            [GovUkRadioCheckboxLabelText(Text = "Under £36 million")]
            Under36Million = 1,

            [GovUkRadioCheckboxLabelText(Text = "£36 million to £60 million")]
            From36to60Million = 2,

            [GovUkRadioCheckboxLabelText(Text = "£60 million to £100 million")]
            From60to100Million = 3,

            [GovUkRadioCheckboxLabelText(Text = "£100 million to £500 million")]
            From100to500Million = 4,

            [GovUkRadioCheckboxLabelText(Text = "Over £500 million")]
            Over500Million = 5,
        }

        #endregion

        public override string PageTitle => "Your organisation";

        public List<short> Sectors { get; set; } = new List<short>();

        [MaxLength(128)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherSector { get; set; }

        [Display(Name = "What was your turnover or budget during the last financial accounting year?")]
        public TurnoverRanges? Turnover { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            var otherId = SectorTypes.Single(x => x.Description.Equals("Other")).Id;

            if (Sectors.Contains(otherId) && string.IsNullOrEmpty(OtherSector))
                validationResults.AddValidationError(3300, nameof(OtherSector));

            return validationResults;
        }

        public override bool IsComplete()
        {
            var other = SectorTypes.Single(x => x.Description.Equals("Other"));

            return Sectors.Any()
                && !Sectors.Any(t => t == other.Id && string.IsNullOrWhiteSpace(OtherSector))
                && Turnover != TurnoverRanges.NotProvided;
        }
    }
}
