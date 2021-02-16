using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class Submission_Complete_All_Sections : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        private Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            //HostHelper.ResetDbScope();



        }

        public Submission_Complete_All_Sections() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());

            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail).ConfigureAwait(false);
            RefreshPage();

            await this.SaveDatabaseAsync().ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            ExpectHeader("Register or select organisations you want to add statements for");

            Click(org.OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");


            Click(The.Top, "Start Draft");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Before you start");


            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Transparency and modern slavery");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");


            Click("Organisations covered by the statement");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: TestData.OrgName);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(42)]
        public async Task StatementURLDateAndSignOff()
        {
            Click("Statement URL, dates and sign-off");
            ExpectHeader("Provide a link to the modern slavery statement on your organisation's website");
            Set("URL").To(Submission.YourMSStatement_URL);

            Click("Save and Continue");

            ExpectHeader("What period does this statement cover?");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_From_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");


            Click("Save and continue");
            ExpectHeader("What is the name of the director (or equivalent) who signed off your statement?");
            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "1");

            Click("Save and Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task AreasCoveredByYourModernSlaveryStatement()
        {
            Click("Recommended areas covered by the statement");
            ExpectHeader("Does your statement cover the following areas in relation to modern slavery?");

            BelowHeader("Your organisation’s structure, business and supply chains").ClickLabel(The.Top, "Yes");
            BelowHeader("Policies").ClickLabel(The.Top, "Yes");
            BelowHeader("Risk assessment").ClickLabel(The.Top, "Yes");
            BelowHeader("Due diligence (steps to address risk)").ClickLabel(The.Top, "Yes");
            BelowHeader("Training about modern slavery").ClickLabel(The.Top, "Yes");
            BelowHeader("Goals and key performance indicators (KPIs) to measure the effectiveness of your actions and progress over time").ClickLabel(The.Top, "Yes");

            Click("Save and Continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(45)]
        public async Task HowManyYears()
        {
            Click("How many years you've been producing statements");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("How many years has your organisation been producing modern slavery statements?");
            ClickLabel("This is the first time");

            Click("Save and continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task YourOrganisation()
        {
            Click("Your organisation's sectors and turnover");
            ExpectHeader(That.Contains, "Which sectors does your organisation operate in?");

            foreach (var sector in Submission.YourOrganisation_Sectors)
            {
                ClickLabel(sector);
            }

            //Set("What was your turnover or budget during the last financial accounting year?").To(Submission.YourOrganisation_Turnover);
            ExpectLabel("Please specify");
            Set("Please specify").To("Other details");
            Click("Save and Continue");
            ExpectHeader("What was your turnover during the financial year the statement relates to?");

            ClickLabel(Submission.YourOrganisation_Turnover);

            Click("Save and continue");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");


            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(48)]
        public async Task Policies()
        {
            Click("Policies");
            ExpectHeader("Do your organisation's policies include any of the following provisions in relation to modern slavery?");

            foreach (var Policy in Submission.Policies_SelectedPolicies)
            {
                ClickLabel(Policy);

                //fill in other details
                if (Policy == "Other")
                {
                    ExpectLabel("Please specify");
                    Set("Please specify").To(Submission.Policies_OtherDetails);
                }
            }

            Click("Save and continue");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(49)]
        public async Task Training()
        {
            Click("Training");

            ExpectHeader("If you provided training on modern slavery during the period of the statement, who was it for?");
            //Training
            Submission_Helper.ChekcboxSelector(this, "If you provided training on modern slavery during the period of the statement, who was it for?", Submission.SelectedTrainings,
                OtherOption: "Other",
                OtherFieldLabel: "Please Specify",
                OtherDetails: Submission.OtherTrainings,
                NeedExpand: false);

            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(50)]
        public async Task MonitoringWorkingConditions()
        {
            Click("Monitoring working conditions");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("During the period of the statement, who did you engage with to help you monitor working conditions across your operations and supply chain?");

            Submission_Helper.ChekcboxSelector(this, "During the period of the statement, who did you engage with to help you monitor working conditions across your operations and supply chain?", Submission.MonitoringProgress_SelectedWhoDidYouEngageWith);
            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Did you use social audits to look for signs of forced labour?");
            Submission_Helper.ChekcboxSelector(this, "Did you use social audits to look for signs of forced labour?", Submission.MonitoringProgress_SelectedSocialAudits);
            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("What types of grievance mechanisms did you have in place?");
            Submission_Helper.ChekcboxSelector(this, "What types of grievance mechanisms did you have in place?", Submission.MonitoringProgress_SelectedGrievanceMechanisms);
            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Are there any other ways you monitored working conditions across your operations and supply chains?");
            Set("Tell us briefly what you did").To(Submission.MonitoringProgress_OtherWays);

            Click("Save and continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(52)]
        public async Task ModernSlaveryRisks()
        {
            Click("Modern slavery risks");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Tell us about your modern slavery risks");

            SetXPath("(//textarea)[1]").To("Risk 1");
            SetXPath("(//textarea)[2]").To("Risk 2");
            SetXPath("(//textarea)[3]").To("Risk 3");


            Click("Save and continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("About this risk");
            Expect("Risk 1");

            BelowHeader("Where was the risk you described most likely to occur?").ClickLabel("Within your own operations");

            BelowHeader("Who was most likely to be affected?").ClickLabel("Women");
            BelowHeader("Who was most likely to be affected?").ClickLabel("Children");

            ClickText("In which country?");
            Type("France");
            ClickText("About this risk"); //clicking the header to finish using the dropdown
            Click("Add country");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectRow("France");

            Below("Tell us about your actions or plans to address this risk").SetXPath("//textarea").To("Risk 1 actions");

            Click("Save and continue");

            //risk 2
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("About this risk");
            Expect("Risk 2");

            BelowHeader("Where was the risk you described most likely to occur?").ClickLabel("Within your supply chains");
            Below("Within your supply chains").ClickLabel("Tier 1 suppliers");
            Below("Within your supply chains").ClickLabel("Tier 2 suppliers");
            Below("Within your supply chains").ClickLabel("Tier 3 suppliers and below");
            Below("Within your supply chains").ClickLabel("Don't know");


            BelowHeader("Who was most likely to be affected?").ClickLabel("Women");
            BelowHeader("Who was most likely to be affected?").ClickLabel("Children");
            BelowHeader("Who was most likely to be affected?").ClickLabel("Migrants");
            BelowHeader("Who was most likely to be affected?").ClickLabel("Refugees");

            ClickText("In which country?");
            Type("Germany");
            ClickText("About this risk"); //clicking the header to finish using the dropdown
            Click("Add country");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectRow("Germany");

            //commentd out curently due to country selection issue
            //todo figure out how to select country properly
            //BelowHeader("In which country?").Set("SelectedCountry").To("Japan");
            //ClickText("Add");
            //await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            //ExpectRow("Japan");

            //BelowHeader("In which country?").Set("SelectedCountry").To("Mexico");
            //ClickText("Add");
            //await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            //ExpectRow("Mexico");
            //BelowHeader("In which country?").Set("SelectedCountry").To("Nigeria");
            //ClickText("Add");
            //await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            //ExpectRow("Nigeria");

            Below("Tell us about your actions or plans to address this risk").SetXPath("//textarea").To("Risk 2 actions");

            Click("Save and continue");

            //risk 3
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("About this risk");
            Expect("Risk 3");

            BelowHeader("Where was the risk you described most likely to occur?").ClickLabel("Other");
            Below("Other").Set(The.Top, "Please specify").To("Other location");


            BelowHeader("Who was most likely to be affected?").ClickLabel("Other vulnerable group(s)");
            Below("Other vulnerable group(s)").Set(The.Top, "Please specify").To("Other groups");

            ClickText("In which country?");
            Type("Ireland");
            ClickText("About this risk"); //clicking the header to finish using the dropdown
            Click("Add country");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            ExpectRow("Ireland");

            Below("Tell us about your actions or plans to address this risk").SetXPath("//textarea").To("Risk 3 actions");

            Click("Save and continue");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(54)]
        public async Task FindingIndicators()
        {
            Click("Finding indicators of modern slavery");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Does your statement refer to finding any International Labour Organization (ILO) indicators of forced labour?");

            foreach (var label in Submission.FindingIndicators_SelectedIndicators)
            {
                ClickLabel(label);
            }
            Click("Save and Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("What action did you take in response?");

            foreach (var label in Submission.FindingIndicators_SelectedActions)
            {
                ClickLabel(label);
            }

            Click("Save and Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(56)]
        public async Task DemonstratingProgress()
        {
            Click("Demonstrating progress");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("How does your statement demonstrate your progress over time in addressing modern slavery risks?");

            SetXPath("//textarea").To("We use KPIs");

            Click("Save and Continue");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(58)]
        public async Task SaveAsDraft()
        {
            Click("Save as draft");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
