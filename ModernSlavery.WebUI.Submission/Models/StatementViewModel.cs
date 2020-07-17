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

        public StatementStatuses Status { get; set; }

        // Date the status last changed
        public DateTime StatusDate { get; set; }

        public DateTime SubmissionDeadline { get; set; }
        // This should never go over the wire!
        public long OrganisationId { get; set; }

        public string OrganisationIdentifier { get; set; }

        public int Year { get; set; }

        public int IncludedOrganistionCount { get; set; }

        public int ExcludedOrganisationCount { get; set; }


        #region Statement Page

        [Url(ErrorMessage = "URL is not valid")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a URL")]
        [MaxLength(255, ErrorMessage = "The web address (URL) cannot be longer than 255 characters.")]
        [Display(Name = "URL")]
        public string StatementUrl { get; set; }
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
        // This is the obfuscated DB id for the org
        [Display(Name = "Job Title")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a job title")]
        public string ApproverJobTitle { get; set; }
        [Display(Name = "First Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a first name")]
        public string ApproverFirstName { get; set; }
        [Display(Name = "Last Name")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter a last name")]
        public string ApproverLastName { get; set; }

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


        #endregion

        #region Compliance Page

        [Required]
        public bool? IncludesStructure { get; set; }
        public string StructureDetails { get; set; }
        [Required]
        public bool? IncludesPolicies { get; set; }
        public string PolicyDetails { get; set; }

        public bool? IncludesRisks { get; set; }
        public string RisksDetails { get; set; }
        [Required]

        public bool? IncludesDueDiligence { get; set; }
        public string DueDiligenceDetails { get; set; }
        [Required]
        public bool? IncludesTraining { get; set; }
        public string TrainingDetails { get; set; }
        [Required]
        public bool? IncludesGoals { get; set; }
        public string GoalsDetails { get; set; }

        #endregion

        #region Your organisation Page
        //TODO: bind StatementSector to this enum with attribute GovUkRadioCheckboxLabelText for the labels
        public List<StatementSectors> Sectors { get; set; }
        public string OtherSector { get; set; }
        public LastFinancialYearBudget? LastFinancialYearBudget { get; set; }

        #endregion

        #region Policies Page
        public List<StatementPolicies> Policies { get; set; }
        [Display(Name = "Please provide detail")]
        public string OtherPolicies { get; set; }

        #endregion

        #region Supply chain risks and due diligence Page 1

        public List<StatementRelevantRisk> RelevantRisks { get; set; }

        public List<StatementRiskType> RelevantRiskTypes { get; set; }
        public string OtherRelevantRisks { get; set; }
        public List<StatementRelevantRisk> HighRisks { get; set; }
        public List<StatementRiskType> HighRiskTypes { get; set; }
        public string OtherHighRisks { get; set; }

        #endregion
        #region Supply chain risks and due diligence Page 2

        public List<StatementDiligence> Diligences { get; set; }
        public List<StatementDiligenceType> DiligenceTypes { get; set; }
        //TODO: clarify mapping to model
        public AnyIdicatorsInSupplyChain? ForcedLabour { get; set; }

        public string ForcedLabourDetails { get; set; }

        //TODO: clarify mapping to model
        public AnyInstancesInSupplyChain? SlaveryInstance { get; set; }

        public string SlaveryInstanceDetails { get; set; }

        public List<StatementRemediation> StatementRemediations { get; set; }
        //TODO: clarify mapping to model
        public string SlaveryInstanceRemediation { get; set; }
        [MaxLength(500)]

        #endregion

        #region Training Page

        public List<StatementTrainings> Training { get; set; }
        [Display(Name = "Please specify")]
        public string OtherTraining { get; set; }

        #endregion

        #region Monitoring progress Page

        public bool IncludesMeasuringProgress { get; set; }

        [Display(Name = "How is your organisation measuring progress towards these goals?")]
        public string ProgressMeasures { get; set; }
        [MaxLength(500)]
        [Display(Name = "What were your key achievements in relation to reducing modern slavery during the period covered by this statement?")]
        public string KeyAchievements { get; set; }

        public NumberOfYearsOfStatements? NumberOfYearsOfStatements { get; set; }

        #endregion



        //this will come from organisation
        public string CompanyName { get; set; }

        //to review how review list will be handled? - for now boolean for each section that can be handled in controller

        [Display(Name = "Your modern slavery statement")]
        public bool IsStatementSectionCompleted { get; set; }

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


    public enum NumberOfYearsOfStatements : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "This is the first time")]
        thisIsTheFirstTime = 0,
        [GovUkRadioCheckboxLabelText(Text = "1 - 5 years")]
        from1To5Years = 1,
        [GovUkRadioCheckboxLabelText(Text = "More than 5 years")]
        moreThan5Years = 2,

    }

    public enum StatementRemediation : byte
    {
        [GovUkRadioCheckboxLabelText(Text = "repayment of recruitment fees")]
        repaymentOfRecruitmentFees,

        [GovUkRadioCheckboxLabelText(Text = "change in policy")]
        changeInPolicy,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

        //etc
    }

    public enum AnyIdicatorsInSupplyChain : byte
    {
        Yes,
        No
    }
    public enum AnyInstancesInSupplyChain : byte
    {
        Yes,
        No
    }

    public enum StatementSectors : byte
    {
        Other = 0
    }
    public enum IncludesGoals
    {
        Yes = 0,
        No = 1
    }
    public enum LastFinancialYearBudget : byte
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

    public enum StatementPolicies : byte
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

    public enum StatementTrainings : byte
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
