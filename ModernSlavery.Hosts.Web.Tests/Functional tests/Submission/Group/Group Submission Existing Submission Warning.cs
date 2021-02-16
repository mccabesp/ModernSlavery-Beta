using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class GroupSubmissionExistingSubmissionWarning : GroupSubmission_Success
    {


        private bool TestRunFailed = false;

        public object Projects { get; private set; }

        [SetUp]
        public void SetUp()
        {
            if (TestRunFailed)
                Assert.Inconclusive("Previous test failed");
            else
                SetupTest(TestContext.CurrentContext.Test.Name);
        }





        [TearDown]
        public void TearDown()
        {

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;

        }
        //DATA NEEDED: Organisation already covered by two or more group statements for submission year 2020-2021 - Fly Jet England published 01/06/2020 and Fly Jet Australia published 01/09/2020

        [Test, Order(68)]
        public async Task RegisterFurtherORgs()
        {


            await this.RegisterUserOrganisationAsync(Organisations[5].OrganisationName, UniqueEmail).ConfigureAwait(false);
            await this.RegisterUserOrganisationAsync(Organisations[6].OrganisationName, UniqueEmail).ConfigureAwait(false);
        }

        [Test, Order(70)]
        public async Task StartDraft()
        {
            Click("Your organisations");

            ExpectHeader("Register or select organisations you want to add statements for");

            Click(Organisations[5].OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[5].OrganisationName);
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

            Click("How many years you've been producing statements");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("How many years has your organisation been producing modern slavery statements?");
            ClickLabel("This is the first time");

            Click("Save and continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);
            await Task.CompletedTask.ConfigureAwait(false);

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

        [Test, Order(72)]
        public async Task NavigateToGroupPage()
        {
            Goto("/manage-organisations");

            Click("Your organisations");
            Expect("Your registered organisations");


            Click(Organisations[6].OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[6].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            RightOfText("2020").BelowText("Do you have to publish a statement on your website by law?").Expect(What.Contains, "Yes");
            await Task.CompletedTask.ConfigureAwait(false);

            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            Click(The.Top, "Start Draft");

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


            Click("Organisations covered by the statement");
            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");

            ClickLabel("A group of organisations");

            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Which organisations are included in your group statement?");

            SetXPath("//input[@class = 'gov-uk-c-searchbar__input']").To(Organisations[0].OrganisationName);
            Click("Search");

            Click(The.Top, "Include");
            ExpectHeader("Which organisations are included in your group statement?");

            SetXPath("//input[@class = 'gov-uk-c-searchbar__input']").To(Organisations[1].OrganisationName);
            Click("Search");

            Click(The.Top, "Include");
            ExpectHeader("Which organisations are included in your group statement?");

            SetXPath("//input[@class = 'gov-uk-c-searchbar__input']").To(Organisations[5].OrganisationName);
            Click("Search");

            Click(The.Top, "Include");
            ExpectHeader("Which organisations are included in your group statement?");

            Click("See which organisations you've selected");
            ExpectHeader("Review the organisations in your group statement");




            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[0].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, "Included in the 2020 statement for");
            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[0].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, Organisations[0].OrganisationName);
            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[0].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, "(submitted to our service on " + DateTime.Now.ToString("d MMM yyyy") + ")");


            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[1].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, "Included in the 2020 statement for");
            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[1].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, Organisations[0].OrganisationName);
            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[1].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, "(submitted to our service on " + DateTime.Now.ToString("d MMM yyyy") + ")");
            //"//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[0]+ "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div


            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[5].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, "Included in the 2020 statement for");
            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[5].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, Organisations[5].OrganisationName);
            AtXPath("//div[@class = 'govuk-inset-text']//p[contains(., '" + Organisations[5].OrganisationName + "') and @class = 'govuk-body govuk-!-margin-bottom-2 govuk-!-font-weight-bold']//parent::div").Expect(What.Contains, "(draft submission in progress on our service)");
        }
    }
}