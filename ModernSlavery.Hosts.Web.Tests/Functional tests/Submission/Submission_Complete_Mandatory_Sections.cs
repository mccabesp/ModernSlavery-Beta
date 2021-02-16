using Geeks.Pangolin;
using Microsoft.Azure.Management.CosmosDB.Fluent.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]


    public class Submission_Complete_Mandatory_Sections : Private_Registration_Success
    {
        //protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            //HostHelper.ResetDbScope();
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());

        }

        [Test, Order(40)]
        public async Task StartSubmission()
        {
            Goto("/manage-organisations");

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
            Click("Save as draft");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}