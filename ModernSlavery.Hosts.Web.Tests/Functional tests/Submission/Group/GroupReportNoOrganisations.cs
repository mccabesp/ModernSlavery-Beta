using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class GroupReportNoOrganisations : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Organisation[] Organisations { get; private set; }
        public GroupReportNoOrganisations() : base(_firstname, _lastname, _title, _email, _password)
        {

        }

        protected Organisation[] organisations = new Organisation[4];
        [OneTimeSetUp]
        public async Task OTSetUp()
        {
            Organisations = this.FindAllUnusedOrgs().ToArray();

            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(6)]
        public async Task RegisterOrg()
        {
            await this.RegisterUserOrganisationAsync(Organisations[0].OrganisationName, UniqueEmail).ConfigureAwait(false);

        }

        [Test, Order(7)]
        public async Task CheckContentForNoOrgReviewPage()
        {
            Goto("/manage-organisations");

            Click("Your organisations");

            Click(Organisations[0].OrganisationName);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: Organisations[0].OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statements");

            RightOfText("2020").BelowText("Do you have to publish a statement on your website by law?").Expect(What.Contains, "No");

            Click(The.Top, "Start Draft");
            ExpectHeader("Before you start");

            Click("Continue");
            ExpectHeader("Transparency and modern slavery");

            Click("Continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            Click("Organisations covered by the statement");
            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");
            ClickLabel("A group of organisations");

            Click("Save and continue");
            ExpectHeader("Which organisations are included in your group statement?");
            Click("Continue");

            ExpectHeader(That.Contains, "Add your 2020 modern slavery statement to the registry");

            AtXPath("//li//span[contains(., '" + "Organisations covered by the statement" + "')]//parent::li").Expect("In Progress");

            Click("Organisations covered by the statement");

            ExpectHeader("You have not added any organisations to your group");
            Expect("Either select 'a single organisation'");
            Expect("Continue as a group, then tell us which organisations are in your group on the next page");
            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");


            //ExpectHeader("Review the organisations included in your group statement");
            //ExpectText("You’ve told us your statement is for the following group of organisations. You can add more organisations to the group, or remove organisations, before confirming and continuing.");
            // ExpectText("If your statement is for a single organisation, and not a group, you can return to the ‘Who is your statement for?’ page and specify a single organisation.");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(8)]
        public async Task CheckSelectOrganisationsNoOrgNavigation()
        {
            Click("Save and continue");
            ExpectHeader("which organisations are included in your group statement?");
            Click("Back");
            ExpectHeader("Does your modern slavery statement cover a single organisation or a group of organisations?");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(9)]
        public async Task CheckSelectOrganisationsConfirmAndContinueNavigation()
        {
            Expect("Save and continue");
            Click("Save and continue");
            ExpectHeader("Which organisations are included in your group statement?");
            Click("Continue");
            ExpectHeader("Add your 2020 modern slavery statement to the registry");

            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
