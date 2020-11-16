using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;
using ModernSlavery.BusinessDomain.Shared.Models;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class GrievancesViewModelMapperProfile : Profile
    {
        public GrievancesViewModelMapperProfile()
        {
            CreateMap<StatementModel, GrievancesViewModel>()
                //.ForMember(d => d.OtherGrievanceMechanisms, opt => opt.MapFrom(s => s.Summary.OtherGrievanceMechanisms))
                .ForMember(d => d.GrievanceMechanisms, opt => opt.MapFrom(s => s.Summary.GrievanceMechanisms));

            CreateMap<GrievancesViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                //.ForPath(d => d.Summary.OtherGrievanceMechanisms, opt => opt.MapFrom(s => s.OtherGrievanceMechanisms))
                .ForPath(d => d.Summary.GrievanceMechanisms, opt => opt.MapFrom(s => s.GrievanceMechanisms));
        }
    }

    public class GrievancesViewModel : BaseViewModel
    {
        public override string PageTitle => "What types of anonymous grievance mechanisms did you have in place?";

        public List<GrievanceMechanismTypes> GrievanceMechanisms { get; set; } = new List<GrievanceMechanismTypes>();

        //[MaxLength(256)]
        //public string OtherGrievanceMechanisms { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            //if (GrievanceMechanisms.Contains(GrievanceMechanismTypes.Other) && string.IsNullOrWhiteSpace(OtherGrievanceMechanisms))
            //    validationResults.AddValidationError(4200, nameof(OtherGrievanceMechanisms));

            if (GrievanceMechanisms.Contains(GrievanceMechanismTypes.None) && GrievanceMechanisms.Count() > 1)
                validationResults.AddValidationError(4201, nameof(GrievanceMechanisms));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (GrievanceMechanisms.Any())
            {
                //if (GrievanceMechanisms.Contains(GrievanceMechanismTypes.Other) && string.IsNullOrWhiteSpace(OtherGrievanceMechanisms)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }

    }
}
