using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModernSlavery.WebUI.Submission.Presenters
{
    [Serializable]
    public class StatementViewModel : GovUkViewModel
    {
        // DB layer Id
        public long StatementId { get; set; }

        // Presentation layer Id
        public string StatementIdentifier { get; set; }

        public ReturnStatuses Status { get; set; }

        // Date the status last changed
        public DateTime StatusDate { get; set; }

        // Earliest date that the submission can be started
        public DateTime StatementStartDate { get; set; }

        public DateTime StatementEndDate { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        // This should never go over the wire!
        public long OrganisationId { get; set; }

        public string OrganisationIdentifier { get; set; }

        public int Year { get; set; }
        [Url]
        [MaxLength(255, ErrorMessage = "The web address (URL) cannot be longer than 255 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public DateTime ApprovedDate { get; set; }
        public AffirmationType IncludesGoals { get; set; }
        [Required]
        public bool IncludesStructure { get; set; }

        public string IncludesStructureDetail { get; set; }
        [Required]
        public bool IncludesPolicies { get; set; }
        public string IncludesPoliciesDetail { get; set; }
        [Required]
        public bool IncludesMethods { get; set; }
        public string IncludesMethodsDetail { get; set; }
        [Required]
        public bool IncludesRisks { get; set; }
        public string IncludesRisksDetail { get; set; }
        [Required]
        public bool IncludesEffectiveness { get; set; }

        public string IncludedEffectivenessDetail { get; set; }
        [Required]
        public bool IncludesTraining { get; set; }
        public string IncludesTrainingDetail { get; set; }

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }

        public string OtherSectorText { get; set; }

        public List<StatementSectors> StatementSectors { get; set; }

        public LastFinancialYearBudget? LastFinancialYearBudget { get; set; }

        public List<StatementPolicies> StatementPolicies { get; set; }
        [Display(Name = "Please provide detail")]
        public string OtherPolicyText { get; set; }
    }
    public enum StatementSectors
    {
        [GovUkRadioCheckboxLabelText(Text = "All")]
        All,

        [GovUkRadioCheckboxLabelText(Text = "Accommodation and food service activities")]
        AccommodationAndFoodServiceActivities,

        [GovUkRadioCheckboxLabelText(Text = "Activities of extraterritorial organisations and bodies")]
        ActivitiesOfExtraterritorialOrganisationsAndBodies,

        [GovUkRadioCheckboxLabelText(Text = "Activities of households as employers")]
        ActivitiesOfHouseholdsAsEmployers,

        [GovUkRadioCheckboxLabelText(Text = "Administrative and support service activities")]
        AdministrativeAndSupportServiceActivities,

        [GovUkRadioCheckboxLabelText(Text = "Agriculture, Forestry and Fishing")]
        AgricultureForestryAndFishing,

        [GovUkRadioCheckboxLabelText(Text = "Arts, entertainment and recreation")]
        ArtsEntertainmentAndRecreation,

        [GovUkRadioCheckboxLabelText(Text = "Construction")]
        Construction,

        [GovUkRadioCheckboxLabelText(Text = "Education")]
        Education,

        [GovUkRadioCheckboxLabelText(Text = "Electricity, gas, steam and air conditioning supply")]
        ElectricityGasSteamAndAirConditioningSupply,

        [GovUkRadioCheckboxLabelText(Text = "Financial and insurance activities")]
        FinancialAndInsuranceActivities,

        [GovUkRadioCheckboxLabelText(Text = "Human health and social work activities")]
        HumanHealthAndSocialWorkActivities,

        [GovUkRadioCheckboxLabelText(Text = "Information and communication")]
        InformationAndCommunication,

        [GovUkRadioCheckboxLabelText(Text = "Manufacturing")]
        Manufacturing,

        [GovUkRadioCheckboxLabelText(Text = "Mining and Quarrying")]
        MiningAndQuarrying,


        [GovUkRadioCheckboxLabelText(Text = "Other service activities")]
        OtherServiceActivities,


        [GovUkRadioCheckboxLabelText(Text = "Professional scientific and technical activities")]
        ProfessionalScientificAndTechnicalActivities,

        [GovUkRadioCheckboxLabelText(Text = "Public administration and defence")]
        PublicAdministrationAndDefence,

        [GovUkRadioCheckboxLabelText(Text = "Public sector")]
        PublicSector,

        [GovUkRadioCheckboxLabelText(Text = "Real estate activities")]
        RealEstateActivities,

        [GovUkRadioCheckboxLabelText(Text = "Transportation and storage")]
        TransportationAndStorage,

        [GovUkRadioCheckboxLabelText(Text = "Water supply, sewerage, waste management and remediation activities")]
        WaterSupplySewerageWasteManagementAndRemediationActivities,

        [GovUkRadioCheckboxLabelText(Text = "Wholesale and retail trade")]
        WholesaleAndRetailTrade,

        //TODO: should we have this, it's on prototype but not wireframes
        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

    }
    public enum LastFinancialYearBudget
    {
        [GovUkRadioCheckboxLabelText(Text = "Under £36 million")]
        Under36Million = 0,

        [GovUkRadioCheckboxLabelText(Text = "£36 million - £60 million")]
        From36MillionTo60Million = 1,

        [GovUkRadioCheckboxLabelText(Text = "£60 million - £100 million")]
        From60MillionTo100Million = 2,

        [GovUkRadioCheckboxLabelText(Text = "£100 million - £500 million")]
        From100MillionTo500Million = 3,

        [GovUkRadioCheckboxLabelText(Text = "£500 million+")]
        From500MillionUpwards = 4,

    }

    public enum StatementPolicies
    {
        [GovUkRadioCheckboxLabelText(Text = "All")]
        All,

        [GovUkRadioCheckboxLabelText(Text = "Adherence to local and national laws")]
        AdherenceToLocalAndNationalLaws,

        [GovUkRadioCheckboxLabelText(Text = "Freedom of workers to terminate employment")]
        FreedomOfWorkersToTerminateEmployment,

        [GovUkRadioCheckboxLabelText(Text = "Freedom of movement")]
        FreedomOfMovement,

        [GovUkRadioCheckboxLabelText(Text = "Freedom of association")]
        FreedomOfAssociation,

        [GovUkRadioCheckboxLabelText(Text = "Prohibits any threat of violence, harassment and intimidation")]
        ProhibitsAnyThreatOfViolenceHarassmentAndIntimidation,

        [GovUkRadioCheckboxLabelText(Text = "Prohibits the use of worker-paid recruitment fees")]
        ProhibitsTheUseOfWorkerPaidRecruitmentFees,

        [GovUkRadioCheckboxLabelText(Text = "Prohibits compulsory overtime")]
        ProhibitsCompulsoryOvertime,

        [GovUkRadioCheckboxLabelText(Text = "ProhibitsChildLabour")]
        ProhibitsChildLabour,

        [GovUkRadioCheckboxLabelText(Text = "Prohibits discrimination")]
        ProhibitsDiscrimination,

        [GovUkRadioCheckboxLabelText(Text = "Prohibits confiscation of workers original identification documents")]
        ProhibitsConfiscationOfWorkersOriginalIdentificationDocuments,

        [GovUkRadioCheckboxLabelText(Text = "Provides access to remedy, compensation and justice for victims of modern slavery")]
        ProvidesAccessToRemedyCompensationAndJusticeForVictimsOfModernSlavery,

        [GovUkRadioCheckboxLabelText(Text = "None of the above")]
        NoneOfTheAbove,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

    }
}
