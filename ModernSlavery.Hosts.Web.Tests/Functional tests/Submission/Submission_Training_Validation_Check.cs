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


    public class Submission_Training_Validation_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());


        }

        public Submission_Training_Validation_Check() : base(_firstname, _lastname, _title, _email, _password)
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
        public async Task SelectOtherOption()
        {
            //if chosing "other" details must be provided
            ClickLabel("Other");
            ClearField("OtherTraining");
            await Task.CompletedTask;
        }

        [Test, Order(44)]
        public async Task SubmittingFormWithoutOptionsCausesValidation()
        {
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            Expect("There is a problem");
            Expect("Missing details");

            BelowField("OtherTraining").Expect("Enter details");
            await Task.CompletedTask;
        }

        [Test, Order(46)]
        public async Task FormCanBeSubmittedWithDetailsGiven()
        {
            Set("OtherTraining").To("details");
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Monitoring progress");
            await Task.CompletedTask;

        }
    }
}