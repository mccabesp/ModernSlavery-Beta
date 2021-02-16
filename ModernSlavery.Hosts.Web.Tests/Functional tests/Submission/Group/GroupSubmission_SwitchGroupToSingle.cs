using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class GroupSubmission_SwitchGroupToSingle : CreateAccount
    {

        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;
        public GroupSubmission_SwitchGroupToSingle() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(30)]
        public async Task RegisterOrg()
        {
            await this.RegisterUserOrganisationAsync(TestData.Organisations[0].OrganisationName, UniqueEmail).ConfigureAwait(false);
            await this.RegisterUserOrganisationAsync(TestData.Organisations[1].OrganisationName, UniqueEmail).ConfigureAwait(false);

            await this.RegisterUserOrganisationAsync(TestData.OrgName, UniqueEmail).ConfigureAwait(false);
        }

        [Test, Order(32)]
        public async Task GoToManageOrgPage()
        {
            Goto("/");

            Click("Your organisations");
            Expect("Your registered organisations");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(34)]
        public async Task SelectOrg()
        {

            Click(TestData.Organisations[0].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            RightOfText("2020").BelowText("Do you have to publish a statement on your website by law?").Expect(What.Contains, "No");
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(35)]
        public async Task ContinueToSubmission()
        {
            Click(The.Top, "Start draft");
            ExpectHeader("Before you start");
            Click("Continue");
            ExpectHeader("Transparency and modern slavery");
            Click("Continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }


        [Test, Order(36)]
        public async Task ChooseGroupSubmission()
        {
            Click("Organisations covered by the statement");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, true, TestData.Organisations[0].OrganisationName, "2020");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(38)]
        public async Task Select4OrgsForGroup()
        {
            for (int i = 1; i < 5; i++)
            {
                Set("Search").To(TestData.Organisations[i].OrganisationName);
                Click("Search");
                ExpectRow(TestData.Organisations[i].OrganisationName);
                AtRow(TestData.Organisations[i].OrganisationName).Expect(TestData.Organisations[i].CompanyNumber);
                AtRow(TestData.Organisations[i].OrganisationName).Click("Include");
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(40)]
        public async Task ViewGroup()
        {
            Click("Continue");
            ExpectHeader("Review the organisations in your group statement");

            for (int i = 0; i < 5; i++)
            {
                BelowHeader(TestData.Organisations[i].OrganisationName).Expect(TestData.Organisations[i].GetAddressString(DateTime.Now));
                BelowHeader(TestData.Organisations[i].OrganisationName).Expect("Company number: " + TestData.Organisations[i].CompanyNumber);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }



        [Test, Order(43)]
        public async Task YourModernSlaveryStatement()
        {
            Click("Continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            Set("URL").To(Submission.YourMSStatement_URL);

            Click("Save and Continue");

            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "1");
            Submission_Helper.DateSet(this, Submission.YourMSStatement_To_Day, Submission.YourMSStatement_To_Month, Submission.YourMSStatement_To_Year, "2");

            Click("Save and continue");
            ExpectHeader("What is the name of the director (or equivalent) who signed off your statement?");
            Set("First name").To(Submission.YourMSStatement_First);
            Set("Last name").To(Submission.YourMSStatement_Last);
            Set("Job title").To(Submission.YourMSStatement_JobTitle);

            Submission_Helper.DateSet(this, Submission.YourMSStatement_ApprovalDate_Day, Submission.YourMSStatement_ApprovalDate_Month, Submission.YourMSStatement_ApprovalDate_Year, "1");

            Click("Save and Continue");
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

            await Task.CompletedTask.ConfigureAwait(false);
        }


        [Test, Order(45)]
        public async Task YourOrganisation()
        {
            Click("Your organisation's sectors and turnover");
            ExpectHeader(That.Contains, "Which sectors does your organisation operate in?");

            foreach (var sector in Submission.YourOrganisation_Sectors)
            {
                ClickLabel(sector);
            }

            ExpectLabel("Please specify");
            Set("Please specify").To("Other details");
            Click("Save and Continue");
            ExpectHeader("What was your turnover during the financial year the statement relates to?");

            ClickLabel(Submission.YourOrganisation_Turnover);

            Click("Save and continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task HowManyYears()
        {
            Click("How many years you've been producing statements");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("How many years has your organisation been producing modern slavery statements?");
            ClickLabel("This is the first time");

            Click("Save and continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

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

        [Test, Order(48)]
        public async Task SwitchToSingleSubmission()
        {
            Click("Organisations covered by the statement");
            ExpectHeader("Review the organisations in your group statement");
            ClickLink("specify a single organisation");

            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");

            ClickLabel("A single organisation");
            Click("Save and continue");

            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            ClickLabel("I understand and agree with the above declaration");
            Click("Submit for publication");

            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(50)]
        public async Task SubmitDraft()
        {
            Expect(What.Contains, "Submission complete");

            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(52)]
        public async Task CheckSubmission()
        {
            Goto("/manage-organisations");

            Click("Your organisations");
            Expect("Your registered organisations");


            Click(TestData.Organisations[0].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            RightOfText("2020").BelowText("Status of statement on the registry").Expect(What.Contains, "Published");


            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(54)]
        public async Task CheckGroupeeSubmissionStatus()
        {
            Goto("/");

            Click("Your organisations");
            Expect("Your registered organisations");


            Click(TestData.Organisations[1].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            RightOfText("2020").BelowText("Status of statement on the registry").Expect("Not started");


            await Task.CompletedTask.ConfigureAwait(false);

        }



    }
}