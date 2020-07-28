﻿using AutoMapper;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.Submission.Classes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using ValidationContext = System.ComponentModel.DataAnnotations.ValidationContext;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using ModernSlavery.Core.Entities;
using System.Text.Json.Serialization;

namespace ModernSlavery.WebUI.Submission.Models.Statement
{
    public class OrganisationPageViewModelMapperProfile : Profile
    {
        public OrganisationPageViewModelMapperProfile()
        {
            CreateMap<StatementModel,OrganisationPageViewModel>();

            CreateMap<OrganisationPageViewModel, StatementModel>(MemberList.Source)
                .ForMember(d => d.SubmissionDeadline, opt => opt.Ignore())
                .ForSourceMember(s => s.SectorTypes, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PageTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.SubTitle, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ReportingDeadlineYear, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.BackUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.CancelUrl, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ContinueUrl, opt => opt.DoNotValidate());
        }
    }

    public class OrganisationPageViewModel : BaseViewModel
    {
        [IgnoreMap]
        public SectorTypeIndex SectorTypes { get; set; }
        public OrganisationPageViewModel(SectorTypeIndex sectorTypes)
        {
            SectorTypes = sectorTypes;
        }

        public OrganisationPageViewModel()
        {

        }

        #region Types
        public enum TurnoverRanges : byte
        {
            //Not Provided
            NotProvided = 0,

            [GovUkRadioCheckboxLabelText(Text = "Under £36 million")]
            Under36Million = 1,

            [GovUkRadioCheckboxLabelText(Text = "£36 million - £60 million")]
            From36to60Million = 2,

            [GovUkRadioCheckboxLabelText(Text = "£60 million - £100 million")]
            From60to100Million = 3,

            [GovUkRadioCheckboxLabelText(Text = "£100 million - £500 million")]
            From100to500Million = 4,

            [GovUkRadioCheckboxLabelText(Text = "£500 million+")]
            Over500Million = 5,
        }

        #endregion

        public override string PageTitle => "Your organisation";

        public List<short> Sectors { get; set; } = new List<short>();

        [MaxLength(1024)]//We need at least one validation annotation otherwise Validate wont execute
        public string OtherSector { get; set; }

        [Display(Name = "What was your turnover or budget during the last financial accounting year?")]
        public TurnoverRanges? Turnover { get; set; }

        public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            //Get the sector types
            SectorTypes = validationContext.GetRequiredService<SectorTypeIndex>();

            var otherId = SectorTypes.Single(x => x.Description.Equals("Other")).Id;

            if (Sectors.Contains(otherId) && string.IsNullOrEmpty(OtherSector))
                yield return new ValidationResult("Please provide other details");
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
