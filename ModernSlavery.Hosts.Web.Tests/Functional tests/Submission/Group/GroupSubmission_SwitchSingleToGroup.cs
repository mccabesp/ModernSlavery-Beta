using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class GroupSubmission_SwitchSingleToGroup : Submission_Complete_Mandatory_Sections

    //Information needing to be added: A second variant organisation to add to grouping Fly Jet Australia
    {
        [OneTimeSetUp]
        public async Task OTSetUp()
        {
            TestData.Organisation = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null);
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)
        }
        public Organisation[] Organisations { get; private set; }

        [SetUp]
        public async Task SetUp()
        {


            Organisations = this.FindAllUnusedOrgs().ToArray();

            org = Organisations[0];

        }
        [Test, Order(60)]
        public async Task RegisterSecondOrg()
        {
            await this.RegisterUserOrganisationAsync(Organisations[1].OrganisationName, UniqueEmail).ConfigureAwait(false);


            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(61)]
        public async Task NavigateToSubmissionPage()
        {
            Click("Your organisations");
            Click(org.OrganisationName);
            Click("Continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }



        [Test, Order(66)]
        public async Task NavigateToConversionPage()
        {
            Click("Organisations covered by the statement");
            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");

            ClickLabel("A group of organisations");

            Click("Save and continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Which organisations are included in your group statement?");

            await Task.CompletedTask.ConfigureAwait(false);
        }



        [Test, Order(68)]
        public async Task AddAndConfirmGroup()
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

        [Test, Order(71)]
        public async Task ViewGroup()
        {
            Click("See which organisations you've selected");
            ExpectHeader("Review the organisations in your group statement");

            for (int i = 1; i < 5; i++)
            {
                //BelowHeader(Organisations[i].OrganisationName).Expect(Organisations[i].GetAddressString(DateTime.Now));
                Below(Organisations[i].OrganisationName).Expect("Company number: " + Organisations[i].CompanyNumber);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(72)]
        public async Task ContinueToSubmission()
        {
            Click("Continue");
            ExpectHeader(That.Contains, "Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }




        [Test, Order(73)]
        public async Task GroupReviewPage()
        {
            Expect("Submission complete for " + Organisations[0].OrganisationName);

            Expect(What.Contains, "You have completed 5 of 11 sections.");
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(74)]
        public async Task ExitAndSaveChanges()
        {
            ClickLabel("I understand and agree with the above declaration");
            Click("Submit for publication");
            Expect(What.Contains, "Submission complete");


            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(75)]
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

        [Test, Order(76)]
        public async Task CheckGroupeeSubmissionStatus()
        {
            Goto("/manage-organisations");

            Click("Your organisations");
            Expect("Your registered organisations");

            //TODO: investigate why Organisations[1] here is not same as it it above in RegisterSecondOrg()
            Click(Organisations[1].OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[1].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            RightOfText("2020").BelowText("Status of statement on the registry").Expect(What.Contains, "Already included in " + Organisations[0].OrganisationName + "’s 2020 group submission, published on " + DateTime.Now.ToString("dd MMM yyyy"));

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}