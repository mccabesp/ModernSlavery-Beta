using AutoMapper;
using ModernSlavery.Core.Entities.StatementSummary;
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
            CreateMap<StatementSummary1, HighestRisksViewModel>()
                .ForMember(d => d.HighRisk1, opt => opt.MapFrom(s => s.Risks.Count < 1 ? null : s.Risks[0].Description))
                .ForMember(d => d.HighRisk1, opt => opt.MapFrom(s => s.Risks.Count < 2 ? null : s.Risks[1].Description))
                .ForMember(d => d.HighRisk1, opt => opt.MapFrom(s => s.Risks.Count < 3 ? null : s.Risks[2].Description));

            CreateMap<HighestRisksViewModel, StatementSummary1>(MemberList.Source)
                .AfterMap((s, d) =>
                {

                    //Create or clear each risk description
                    foreach (var pars in new[] { (s.HighRisk1, 0), (s.HighRisk2, 1), (s.HighRisk3, 2) })
                        SetRiskDetails(pars.Item1, pars.Item2);

                    void SetRiskDetails(string description, int index)
                    {
                        //Ensure there is a risk for every item
                        var risk = d.Risks.Count <= index ? null : d.Risks[index];
                        if (risk == null)
                        {
                            risk = new IStatementSummary1.StatementRisk();
                            d.Risks.Add(risk);
                        }

                        //Clearing the associated details when the description is cleared
                        if (string.IsNullOrWhiteSpace(description))
                        {
                            risk.LikelySource = IStatementSummary1.StatementRisk.RiskSourceTypes.Unknown;
                            risk.OtherLikelySource = null;
                            risk.Targets.Clear();
                            risk.OtherTargets = null;
                            risk.Countries.Clear();
                        }

                        //Set the new description
                        risk.Description = description;
                    }

                    //Remove empty risks
                    for (var i = d.Risks.Count - 1; i >= 0; i--)
                        if (d.Risks[i].IsEmpty()) d.Risks.RemoveAt(i);
                })
                .ForAllMembers(opt => opt.Ignore());
        }


    }

    public class HighestRisksViewModel : BaseViewModel
    {
        public override string PageTitle => "Tell us about your highest risks";

        [MaxLength(200)]
        public string HighRisk1 { get; set; }


        [MaxLength(200)]
        public string HighRisk2 { get; set; }

        [MaxLength(200)]
        public string HighRisk3 { get; set; }

        public bool None { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            return validationResults;
        }

        public override Status GetStatus()
        {
            if (!string.IsNullOrWhiteSpace(HighRisk1) || !string.IsNullOrWhiteSpace(HighRisk2) || !string.IsNullOrWhiteSpace(HighRisk3))
            {
                if (!string.IsNullOrWhiteSpace(HighRisk1) && !string.IsNullOrWhiteSpace(HighRisk2) && !string.IsNullOrWhiteSpace(HighRisk3)) return Status.Complete;
                    return Status.InProgress;
            }

            return Status.Incomplete;
        }
    }
}
