using Geeks.Pangolin;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Submission_Sectors_Interaction_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        protected  Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());


        }

        public Submission_Sectors_Interaction_Check() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task NavigateToSectorsPage()
        {

            Submission_Helper.NavigateToYourOrganisation(this, org.OrganisationName, "2019 to 2020", MoreInfoRequired: true);
            await AxeHelper.CheckAccessibilityAsync(this);
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task ContinueLeadsToAreasPage()
        {
            ExpectHeader("Your organisation");

            ExpectHeader("Which sector does your organisation operate in?");


            //expect all sectors in order
            Submission_Helper.ExpectSectors(this, Submission.Sectors);

            //expect all financial options in order
            Submission_Helper.ExpectFinancials(this, Submission.Financials);

            Submission_Helper.SectorsInteractionCheck(this, Submission.Sectors);
            Submission_Helper.FinancialsInteractionCheck(this, Submission.Financials);

        }
    }
}