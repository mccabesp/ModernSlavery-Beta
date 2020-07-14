using ModernSlavery.Core.Entities;
using ModernSlavery.WebUI.GDSDesignSystem.Attributes;
using ModernSlavery.WebUI.GDSDesignSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;

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

        public DateTime? StatementStartDate
        {
            get
            {
                return ParseDate(StatementStartYear, StatementStartMonth, StatementStartDay);
            }
            set
            {
                if (value == null)
                {
                    StatementStartYear = null;
                    StatementStartMonth = null;
                    StatementStartDay = null;
                }
                else
                {
                    StatementStartYear = value.Value.Year;
                    StatementStartMonth = value.Value.Month;
                    StatementStartDay = value.Value.Day;
                }
            }
        }

        // Earliest date that the submission can be started
        public int? StatementStartDay { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a start month")]
        public int? StatementStartMonth { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a start year")]
        public int? StatementStartYear { get; set; }

        public DateTime? StatementEndDate
        {
            get
            {
                return ParseDate(StatementEndYear, StatementEndMonth, StatementEndDay);
            }
            set
            {
                if (value == null)
                {
                    StatementEndYear = null;
                    StatementEndMonth = null;
                    StatementEndDay = null;
                }
                else
                {
                    StatementEndYear = value.Value.Year;
                    StatementEndMonth = value.Value.Month;
                    StatementEndDay = value.Value.Day;
                }
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter an end day")]
        public int? StatementEndDay { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter an end month")]
        public int? StatementEndMonth { get; set; }
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter an end year")]
        public int? StatementEndYear { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        // This should never go over the wire!
        public long OrganisationId { get; set; }

        public string OrganisationIdentifier { get; set; }

        public int Year { get; set; }

        [Url(ErrorMessage = "URL is not valid")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a URL")]
        [MaxLength(255, ErrorMessage = "The web address (URL) cannot be longer than 255 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }
        [Display(Name = "Job Title")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a job title")]
        public string JobTitle { get; set; }
        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a first name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a last name")]
        public string LastName { get; set; }

        public DateTime? ApprovedDate
        {
            get
            {
                return ParseDate(ApprovedYear, ApprovedMonth, ApprovedDay);
            }
            set
            {
                if (value == null)
                {
                    ApprovedYear = null;
                    ApprovedMonth = null;
                    ApprovedDay = null;
                }
                else
                {
                    ApprovedYear = value.Value.Year;
                    ApprovedMonth = value.Value.Month;
                    ApprovedDay = value.Value.Day;
                }
            }
        }

        public int? ApprovedDay { get; set; }
        public int? ApprovedMonth { get; set; }
        public int? ApprovedYear { get; set; }

        public bool IncludesGoals { get; set; }
        [Required]
        public bool? IncludesStructure { get; set; }

        public string IncludesStructureDetail { get; set; }
        [Required]
        public bool? IncludesPolicies { get; set; }
        public string IncludesPoliciesDetail { get; set; }
        [Required]
        public bool? IncludesMethods { get; set; }
        public string IncludesMethodsDetail { get; set; }
        [Required]
        public bool? IncludesRisks { get; set; }
        public string IncludesRisksDetail { get; set; }
        [Required]
        public bool? IncludesEffectiveness { get; set; }

        public string IncludedEffectivenessDetail { get; set; }
        [Required]
        public bool? IncludesTraining { get; set; }
        public string IncludesTrainingDetail { get; set; }

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }

        public string OtherSectorText { get; set; }

        public List<StatementSectors> StatementSectors { get; set; }

        public LastFinancialYearBudget? LastFinancialYearBudget { get; set; }

        public List<StatementPolicies> StatementPolicies { get; set; }
        [Display(Name = "Please provide detail")]
        public string OtherPolicyText { get; set; }

        public List<StatementTrainings> StatementTrainings { get; set; }
        [Display(Name = "Please specify")]
        public string OtherTrainingText { get; set; }

        public List<StatementRelevantRisk> StatementRisks { get; set; }

        public List<StatementRiskType> StatementRiskTypes { get; set; }

        public string OtherRiskText { get; set; }

        public List<Continent> Continents { get; set; }

        public List<Core.Classes.Country> Countries { get; set; }

        public List<StatementDiligence> StatementDiligences { get; set; }
        public List<StatementDiligenceType> StatementDiligenceTypes { get; set; }

        //need to find corresponsing model property
        public AnyIdicatorsInSupplyChain? AnyIdicatorsInSupplyChain { get; set; }

        //need to find corresponsing model property

        public string IndicatorDetails { get; set; }

        //need to find corresponsing model property
        public List<AnyInstancesInSupplyChain> AnyInstancesInSupplyChain { get; set; }

        //need to find corresponsing model property
        public string InstanceDetails { get; set; }

        public List<StatementRemediation> StatementRemediations { get; set; }

        public string OtherRemediationText { get; set; }
        [MaxLength(500)]
        [Display(Name = "How is your organisation measuring progress towards these goals?")]
        public string MeasuringProgress { get; set; }
        [MaxLength(500)]
        [Display(Name = "What were your key achievements in relation to reducing modern slavery during the period covered by this statement?")]
        public string KeyAchievements { get; set; }

        public List<NumberOfYearsOfStatements> NumberOfYearsOfStatements { get; set; }

        //this will come from organisation
        public string CompanyName { get; set; }

        //to review how review list will be handled? - for now boolean for each section that can be handled in controller
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Your modern slavery statement")]
        public bool IsStatementSectionCompleted { get; set; }
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Areas covered by your modern slavery statement")]
        public bool IsAreasCoveredSectionCompleted { get; set; }
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Your organisation")]
        public bool IsOrganisationSectionCompleted { get; set; }
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Policies")]
        public bool IsPoliciesSectionCompleted { get; set; }
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Supply chain risks and due diligence (part 1)")]
        public bool IsSupplyChainRiskAndDiligencPart1SectionCompleted { get; set; }
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Supply chain risks and due diligence (part 2)")]
        public bool IsSupplyChainRiskAndDiligencPart2SectionCompleted { get; set; }
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Training")]
        public bool IsTrainingSectionCompleted { get; set; }
        [UIHint("CompletedNotCompleted")]
        [Display(Name = "Monitoring Progress")]
        public bool IsMonitoringProgressSectionCompleted { get; set; }


        private DateTime? ParseDate(int? year, int? month, int? day)
        {
            if (!year.HasValue || !month.HasValue || !day.HasValue)
                return null;

            DateTime result;
            if (DateTime.TryParse($"{year}-{month}-{day}", out result))
                return result;

            return null;
        }
    }


    public enum NumberOfYearsOfStatements
    {
        [GovUkRadioCheckboxLabelText(Text = "This is the first time")]
        thisIsTheFirstTime = 0,
        [GovUkRadioCheckboxLabelText(Text = "1 - 5 years")]
        from1To5Years = 1,
        [GovUkRadioCheckboxLabelText(Text = "More than 5 years")]
        moreThan5Years = 2,

    }

    public enum StatementRemediation
    {
        [GovUkRadioCheckboxLabelText(Text = "repayment of recruitment fees")]
        repaymentOfRecruitmentFees,

        [GovUkRadioCheckboxLabelText(Text = "change in policy")]
        changeInPolicy,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

        //etc
    }

    public enum AnyIdicatorsInSupplyChain
    {
        Yes,
        No
    }
    public enum AnyInstancesInSupplyChain
    {
        Yes,
        No
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

    public enum StatementTrainings
    {
        [GovUkRadioCheckboxLabelText(Text = "All")]
        All,

        [GovUkRadioCheckboxLabelText(Text = "Procurement")]
        Procurement,

        [GovUkRadioCheckboxLabelText(Text = "Human Resources")]
        HumanResources,

        [GovUkRadioCheckboxLabelText(Text = "C-Suite")]
        CSuite,

        [GovUkRadioCheckboxLabelText(Text = "Whole organisation")]
        WholeOrganisation,

        [GovUkRadioCheckboxLabelText(Text = "Suppliers")]
        Suppliers,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

    }
}
