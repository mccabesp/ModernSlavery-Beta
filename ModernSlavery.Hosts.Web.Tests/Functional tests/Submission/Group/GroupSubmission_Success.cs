using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class GroupSubmission_Success : CreateAccount
    {

        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        public Organisation[] Organisations { get; private set; }

        string Pin;
        public GroupSubmission_Success() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        protected Organisation[] organisations = new Organisation[4];
        [OneTimeSetUp]
        public async Task OTSetUp()
        {
            Organisations = this.FindAllUnusedOrgs().ToArray();

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(30)]
        public async Task RegisterOrg()
        {
            await this.RegisterUserOrganisationAsync(Organisations[0].OrganisationName, UniqueEmail).ConfigureAwait(false);
            await this.RegisterUserOrganisationAsync(Organisations[1].OrganisationName, UniqueEmail).ConfigureAwait(false);
        }

        [Test, Order(32)]
        public async Task GoToManageOrgPage()
        {
            Goto("/manage-organisations");

            Click("Your organisations");
            Expect("Your registered organisations");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(34)]
        public async Task SelectOrg()
        {

            Click(Organisations[0].OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[0].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            RightOfText("2020").BelowText("Do you have to publish a statement on your website by law?").Expect(What.Contains, "No");
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(35)]
        public async Task StartSubmission()
        {

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
        [Test, Order(36)]
        public async Task ChooseGroupSubmission()
        {
            Click("Organisations covered by the statement");
            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");

            ClickLabel("A group of organisations");

            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Which organisations are included in your group statement?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(38)]
        public async Task Select4OrgsForGroup()
        {
            for (int i = 1; i < 5; i++)
            {
                SetXPath("//input[@class = 'gov-uk-c-searchbar__input']").To(Organisations[i].OrganisationName);
                Click("Search");
                Below(What.Contains, "Can't find the organisation you're looking for?").Expect(Organisations[i].OrganisationName);
                Below(What.Contains, "Can't find the organisation you're looking for?").RightOf(Organisations[i].OrganisationName).Expect(Organisations[i].CompanyNumber);
                Below(What.Contains, "Can't find the organisation you're looking for?").RightOf(Organisations[i].OrganisationName).Click(The.Top, "Include");
                //Expect(i + "  organisations included");
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(40)]
        public async Task ViewGroup()
        {
            //            Expect("4  organisations included");
            Click("See which organisations you've selected");
            ExpectHeader("Review the organisations in your group statement");

            for (int i = 1; i < 5; i++)
            {
                //BelowHeader(Organisations[i].OrganisationName).Expect(Organisations[i].GetAddressString(DateTime.Now));
                Below(Organisations[i].OrganisationName).Expect("Company number: " + Organisations[i].CompanyNumber);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(41)]
        public async Task ContinueToSubmission()
        {
            Click("Continue");
            //ExpectHeader("Before you start");
            //Click("Start Now");
            ExpectHeader(That.Contains, "Add your 2020 modern slavery statement to the registry");

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

        [Test, Order(47)]
        public async Task OptionalSectionsIncomplete()
        {
            var MandatorySections = new string[] {
            "Organisations covered by the statement",
            "Statement URL, dates and sign-off",
            "Recommended areas covered by the statement",
            "Your organisation", //abbereviated due to ' in string
            "How many years you" //abbereviated due to ' in string
            };

            var OptionalSections = new string[] {
            "Policies",
            "Training",
            "Monitoring working conditions",
            "Modern slavery risks",
            "Finding indicators of modern slavery",
            "Demonstrating progress"
            };


            //mandaotry sections should be completed
            foreach (var mandatorySection in MandatorySections)
            {
                Submission_Helper.SectionCompleteionCheck(this, true, mandatorySection);
            }

            //optional sections incomplete
            foreach (var optionalSection in OptionalSections)
            {
                Submission_Helper.SectionCompleteionCheck(this, false, optionalSection);
            }
            await Task.CompletedTask.ConfigureAwait(false);
        }


        [Test, Order(58)]
        public async Task SaveAsDraft()
        {
            ClickLabel("I understand and agree with the above declaration");
            Click("Submit for publication");


            ExpectHeader("Submission complete");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(60)]
        public async Task CheckSubmission()
        {
            Goto("/manage-organisations");

            Click("Your organisations");
            Expect("Your registered organisations");


            Click(Organisations[0].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            DateTime now = DateTime.Now;

            RightOfText("2020").BelowText("Status of statement on the registry").Expect(What.Contains, "Published");


            RightOfText("2020").BelowText("Status of statement on the registry").Expect(What.Contains, "on " + now.Day + " " + now.ToString("MMMM") + " " + now.Year);


            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(62)]
        public async Task CheckGroupeeSubmissionStatus()
        {
            Goto("/manage-organisations");

            Click("Your organisations");
            Expect("Your registered organisations");


            Click(Organisations[1].OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[1].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            //RightOfText("2020").BelowText("Status of statement on the registry").Expect(What.Contains, "Already included in "+ Organisations[0].OrganisationName + "’s 2020 group submission, published on " + DateTime.Now.ToString("dd MMM yyyy"));

            AtRow("2020").Expect(What.Contains, "Already included in " + Organisations[0].OrganisationName + "'s 2020 group submission, published on " + DateTime.Now.ToString("d MMM yyyy"));

            AtRow("2019").Expect(What.Contains, "Not Started");
            await Task.CompletedTask.ConfigureAwait(false);
        }



    }
}