using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GrievancesPageViewModelMapperProfile : Profile
    {
        public GrievancesPageViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, GrievancesPageViewModel>();

            CreateMap<GrievancesPageViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.GrievanceMechanisms, opt => opt.MapFrom(s=>s.GrievanceMechanisms))
                .ForMember(d => d.OtherGrievanceMechanisms, opt => opt.MapFrom(s=>s.OtherGrievances))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class GrievancesPageViewModel : BaseViewModel
    {
        public override string PageTitle => "What types of anonymous grievance mechanisms do you have in place?";

        public List<GrievanceMechanismTypes> GrievanceMechanisms { get; set; } = new List<GrievanceMechanismTypes>();

        [MaxLength(256)]
        public string OtherGrievances { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (GrievanceMechanisms.Contains(GrievanceMechanismTypes.Other) && string.IsNullOrWhiteSpace(OtherGrievances))
                validationResults.AddValidationError(4200, nameof(OtherGrievances));

            return validationResults;
        }

        public override bool IsComplete()
        {
            return GrievanceMechanisms.Any()
                && (!GrievanceMechanisms.Contains(GrievanceMechanismTypes.Other) || !string.IsNullOrWhiteSpace(OtherGrievances));
        }
    }
}
