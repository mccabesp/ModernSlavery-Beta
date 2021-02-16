using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities.StatementSummary.V1;
using ModernSlavery.WebUI.Shared.Classes.Attributes;
using ModernSlavery.WebUI.Shared.Classes.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class HighestRisksViewModelMapperProfile : Profile
    {
        public HighestRisksViewModelMapperProfile()
        {
            CreateMap<StatementModel, HighestRisksViewModel>()
                .ForMember(d => d.HighRisk1, opt => opt.MapFrom(s => s.Summary.Risks.Count < 1 ? null : s.Summary.Risks[0].Description))
                .ForMember(d => d.HighRisk2, opt => opt.MapFrom(s => s.Summary.Risks.Count < 2 ? null : s.Summary.Risks[1].Description))
                .ForMember(d => d.HighRisk3, opt => opt.MapFrom(s => s.Summary.Risks.Count < 3 ? null : s.Summary.Risks[2].Description))
                .ForMember(d => d.NoRisks, opt => opt.MapFrom(s => s.Summary.NoRisks));

            CreateMap<HighestRisksViewModel, StatementModel>(MemberList.None)
                .ForMember(d => d.OrganisationId, opt => opt.Ignore())
                .ForMember(d => d.OrganisationName, opt => opt.Ignore())
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForPath(d => d.Summary.NoRisks, opt => opt.MapFrom(s => s.NoRisks))
                .AfterMap((s, d) =>
                {
                    //Clear all risks if no risks selected
                    if (s.NoRisks) s.HighRisk1 = s.HighRisk2 = s.HighRisk3 = null;

                    //Create or clear each risk description
                    foreach (var pars in new[] { (s.HighRisk1, 0), (s.HighRisk2, 1), (s.HighRisk3, 2) })
                        SetRiskDetails(pars.Item1, pars.Item2);

                    void SetRiskDetails(string description, int index)
                    {
                        //Ensure there is a risk for every item
                        var risk = d.Summary.Risks.Count <= index ? null : d.Summary.Risks[index];
                        if (risk == null)
                        {
                            risk = new StatementSummary.StatementRisk();
                            d.Summary.Risks.Add(risk);
                        }

                        //Clearing the associated details when the description is cleared
                        if (string.IsNullOrWhiteSpace(description))
                        {
                            risk.LikelySource = StatementSummary.StatementRisk.RiskSourceTypes.Unknown;
                            risk.OtherLikelySource = null;
                            risk.Targets.Clear();
                            risk.OtherTargets = null;
                            risk.Countries.Clear();
                        }

                        //Set the new description
                        risk.Description = description;
                    }

                    //Remove empty risks
                    for (var i = d.Summary.Risks.Count - 1; i >= 0; i--)
                        if (d.Summary.Risks[i].IsEmpty()) d.Summary.Risks.RemoveAt(i);
                });
        }


    }

    public class HighestRisksViewModel : BaseStatementViewModel
    {
        public override string PageTitle => "Tell us about your modern slavery risks";

        [MaxLength(200)]
        [Text] 
        public string HighRisk1 { get; set; }

        [MaxLength(200)]
        [Text] 
        public string HighRisk2 { get; set; }

        [MaxLength(200)]
        [Text] 
        public string HighRisk3 { get; set; }

        public bool NoRisks { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            if (NoRisks && (!string.IsNullOrWhiteSpace(HighRisk1) || !string.IsNullOrWhiteSpace(HighRisk2) || !string.IsNullOrWhiteSpace(HighRisk3)))
                validationResults.AddValidationError(3913, nameof(NoRisks));

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (NoRisks)
                return Status.Complete;

            if (!string.IsNullOrWhiteSpace(HighRisk1) || !string.IsNullOrWhiteSpace(HighRisk2) || !string.IsNullOrWhiteSpace(HighRisk3))
            {
                if (!string.IsNullOrWhiteSpace(HighRisk1) && !string.IsNullOrWhiteSpace(HighRisk2) && !string.IsNullOrWhiteSpace(HighRisk3)) return Status.Complete;
                return Status.InProgress;
            }

            return Status.Incomplete;
        }
    }
}
