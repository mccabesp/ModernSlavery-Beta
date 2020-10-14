using Geeks.Pangolin;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ModernSlavery.Core.Entities;
using ModernSlavery.Testing.Helpers.Extensions;
using System.Linq;

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture]


    public class Scope_Out_Mark_Org_As_IS_LoggedIn : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        string Pin;

        protected readonly RegistrationTestData TestData;
        protected Organisation org;
        public Scope_Out_Mark_Org_As_IS_LoggedIn() : base(_firstname, _lastname, _title, _email, _password)
        {
            TestData = new RegistrationTestData(this);
        }

        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null);
            //&& !o.UserOrganisations.Any(uo => uo.PINConfirmedDate != null)
        }

        private bool CanBeSetOutOfScope(Organisation org)
            => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null;

        [Test, Order(29)]
        public async Task RegisterOrgAndSetScope()
        {
           
            await this.RegisterUserOrganisationAsync(org.OrganisationName, UniqueEmail);
            await this.GetOrganisationBusinessLogic().CreateOrganisationSecurityCodeAsync(org.OrganisationReference, new DateTime(2030, 1, 10));

            User CurrentUser = await ServiceScope.GetDataRepository().SingleOrDefaultAsync<User>(o => o.EmailAddress == UniqueEmail);
            await this.GetOrganisationBusinessLogic().SetAsScopeAsync(org.OrganisationReference, 2020, "Updated by test case", CurrentUser, ScopeStatuses.OutOfScope, true);


            await Task.CompletedTask;
        }

        [Test, Order(30)]
        public async Task GoToManageOrgPage()
        {
            Goto("/manage-organisations");
            await AxeHelper.CheckAccessibilityAsync(this);

            Click("Manage organisations");
            ExpectHeader(That.Contains, "Select an organisation");

            await Task.CompletedTask;
        }

        [Test, Order(31)]
        public async Task SelectOrg()
        {

            Click(org.OrganisationName);

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            RightOfText("2019 to 2020").BelowText("Required by law to publish a statement on your website?").Expect(What.Contains, "No");
            await Task.CompletedTask;
        }

        [Test, Order(32)]
        public async Task ChangeOrgStatus()
        {
            Click(The.Top, "Change"); 
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Confirm your organisation is required to publish a modern slavery statement");
            await Task.CompletedTask;
        }

        
        [Test, Order(34)]
        public async Task VerifyOrgDetails()
        {
            RightOfText("Organisation Reference").Expect(org.OrganisationReference);
            RightOfText("Organisation Name").Expect(org.OrganisationName);
            //todo await helper implementation for address logic
            //RightOfText("Registered address").Expect("");


            await Task.CompletedTask;
        }

        [Test, Order(34)]
        public async Task CheckOtherContent()
        {
            Expect("Please confirm your organisation is required to publish a modern slavery statement.");
            Expect(What.Contains, "If your organisation name or address is incorrect, email us at");
            ExpectLink(That.Contains, "modernslaverystatements@homeoffice.gov.uk");
            Expect(What.Contains, "and let us know the correct information.");

            await Task.CompletedTask;
        }

       

        [Test, Order(38)]
        public async Task ConfirmChange()
        {
            Click("Confirm");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");
            await Task.CompletedTask;
        }

        [Test, Order(40)]
        public async Task VerifyChange()
        {

            RightOfText("2019 to 2020").BelowText("Required by law to publish a statement on your website?").Expect(What.Contains, "Yes");
            await Task.CompletedTask;
        }

        
    }
}