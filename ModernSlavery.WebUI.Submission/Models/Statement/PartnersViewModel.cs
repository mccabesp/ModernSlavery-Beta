﻿using AutoMapper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using static ModernSlavery.Core.Entities.StatementSummary.IStatementSummary1;
using ModernSlavery.Core.Entities.StatementSummary;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class PartnersViewModelMapperProfile : Profile
    {
        public PartnersViewModelMapperProfile()
        {
            CreateMap<StatementSummary1, PartnersViewModel>();

            CreateMap<PartnersViewModel, StatementSummary1>(MemberList.Source)
                .ForMember(d => d.Partners, opt => opt.MapFrom(s=>s.Partners))
                .ForMember(d => d.OtherPartners, opt => opt.MapFrom(s=>s.OtherPartners))
                .ForAllOtherMembers(opt => opt.Ignore());
        }
    }

    public class PartnersViewModel : BaseViewModel
    {
        public override string PageTitle => "During the period of the statement, who did you engage with to help you monitor working conditions across your organisation and supply chain?";

        public List<PartnerTypes> Partners { get; set; } = new List<PartnerTypes>();

        [MaxLength(256)]
        public string OtherPartners { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (Partners.Contains(PartnerTypes.Other) && string.IsNullOrWhiteSpace(OtherPartners))
                validationResults.AddValidationError(4000, nameof(OtherPartners));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (Partners.Any())
            {
                if (Partners.Contains(PartnerTypes.Other) && string.IsNullOrWhiteSpace(OtherPartners)) return Status.InProgress;
                return Status.Complete;
            }

            return Status.Incomplete;
        }

    }
}