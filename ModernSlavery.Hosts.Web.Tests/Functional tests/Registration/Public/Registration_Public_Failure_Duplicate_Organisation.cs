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

    public class Registration_Public_Failure_Duplicate_Organisation : Registration_Public_Success
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;


        protected Organisation org;
        [OneTimeSetUp]
        public async Task SetUp()
        {
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());
        }
        [Test, Order(40)]
        public async Task SearchForDuplicateOrg()
        {
            Goto("/");

            await NavigateToOrgPage();
            await SearchForOrg();

            await Task.CompletedTask; 
        }
        [Test, Order(42)]
        public async Task SelectDuplicateOrg ()        
        {
            
            ClickButton(That.Contains, "Choose");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            //org already registered
            //error should appear

            Expect("The following errors were detected");
            Expect("You have already started registering for this organisation");

            await Task.CompletedTask;

        }
    }
}