using Geeks.Pangolin;
using Geeks.Pangolin.Helper.UIContext;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Review_And_Edit_Link_Navigation : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());


        }

        public Review_And_Edit_Link_Navigation() : base(_firstname, _lastname, _title, _email, _password)
        {


        }

        [Test, Order(29)]
        public async Task RegisterOrg()
        {
            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail);
            RefreshPage();

            await Task.CompletedTask;
        }
        [Test, Order(40)]
        public async Task NavigateToReviewAndEdit()
        {

            Submission_Helper.NavigateToSubmission(this, org.OrganisationName, "2020", MoreInfoRequired: true);
            await AxeHelper.CheckAccessibilityAsync(this);
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task ExpectLinks()
        {
            //expect links
            //todo add try command once implemented
            ExpectLink("Your modern slavery statement");
            ExpectLink("Areas covered by your modern slavery statement");
            ExpectLink("Your organisation");
            ExpectLink("Policies");
            ExpectLink("Supply chain risks and due diligence (part 1)");
            ExpectLink("Supply chain risks and due diligence (part 2)");
            ExpectLink("Training");
            ExpectLink("Monitoring progress");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task TestLinkNavigation()
        {
            //following link to section on this page should return to this screen once completed
            SaveAndCancelcheck(this, "Your modern slavery statement", "Your modern slavery statement", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Areas covered by your modern slavery statement", "Areas covered by your modern slavery statement", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Your organisation", "Your organisation", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Policies", "Policies", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Supply chain risks and due diligence (part 1)", "Supply Chain Risks and due diligence", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Supply chain risks and due diligence (part 2)", "Supply Chain Risks and due diligence", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Training", "Training", Submission.OrgName_Blackpool);
            SaveAndCancelcheck(this, "Monitoring progress", "Monitoring progress", Submission.OrgName_Blackpool);
           
            await Task.CompletedTask;
        }

        private static void SaveAndCancelcheck(UIContext ui, string LinkText, string HeaderText, string OrgName) 
        { 
            //navigate to page
            ui.ExpectHeader("Review before submitting");
            ui.ClickLink(LinkText);
            ui.ExpectHeader(HeaderText);

            //test cancel/save cancel/cancel and continue all return to draft page
            ui.Click("Continue");
            ui.ExpectHeader("Review before submitting");


            ui.ClickLink(LinkText);
            ui.ExpectHeader(HeaderText);
            ui.Click("Cancel");
            ui.Expect("You have unsaved data, what do you want to do?");
            ui.Click("Exit and lose changes");
            ui.ExpectHeader("Review before submitting");

            ui.ClickLink(LinkText);
            ui.ExpectHeader(HeaderText);
            ui.Click("Cancel");
            ui.Expect("You have unsaved data");
            ui.Click("Save the data");
            ui.Expect(What.Contains, "You`ve saved a draft of your Modern Slavery Statement");
            ui.Click("Continue");
            ui.ExpectHeader("Review before submitting");

        }
    }
}