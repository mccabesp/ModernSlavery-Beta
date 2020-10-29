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


    public class Submission_Training_Content_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());


        }

        public Submission_Training_Content_Check() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task NavigateToTrainingPage()
        {
            Submission_Helper.NavigateToTraining(this, org.OrganisationName, "2020", MoreInfoRequired: true);
            await AxeHelper.CheckAccessibilityAsync(this);

            ExpectHeader("Training");
            await Task.CompletedTask;
        }



        [Test, Order(42)]
        public async Task CheckTrainingPageText()
        {
            Expect("Have you provided training on modern slavery and trafficking during the past year, or any other activities to raise awareness? If so, who was this for?");
            Expect("select all that apply");


            ExpectButton("Continue");
            ExpectButton("Cancel");

            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task CheckTrainingOptions()
        {
            ExpectLabel("Procurement");
            ExpectLabel("Human Resources");
            ExpectLabel("Executive level");
            ExpectLabel("Whole organisation");
            ExpectLabel("Suppliers");
            ExpectLabel("Other");



            await Task.CompletedTask;
        }
        [Test, Order(46)]
        public async Task ExpectOtherDetailsField()
        {
            ClickLabel("Other");
            ExpectLabel("Please specify"); 
            await Task.CompletedTask;
        }
    }
}