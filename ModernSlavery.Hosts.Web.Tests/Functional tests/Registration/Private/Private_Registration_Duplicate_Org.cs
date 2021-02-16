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
    public class Private_Registration_Duplicate_Org : Private_Registration_Success
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        private Organisation org;
        [OneTimeSetUp]
        public async Task OTSetUp()
        {
            //HostHelper.ResetDbScope();
            org = this.Find<Organisation>(org => org.GetLatestActiveScope().ScopeStatus.IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.PresumedInScope) && org.LatestRegistrationUserId == null && !org.UserOrganisations.Any());
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(30)]
        public async Task SearchForOrg()
        {
            Goto("/");
            await GoToPrivateRegistrationPage().ConfigureAwait(false);
            await SearchForOrganisation().ConfigureAwait(false);

            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(31)]

        public async Task SelectDuplicateOrg() { 
            AtRow(That.Contains, org.OrganisationName).Click(What.Contains, "Choose");
            await Task.CompletedTask.ConfigureAwait(false);

        }
        [Test, Order(32)]

        public async Task ExpectErrorMessages() {

            Expect("There is a problem");
            Expect("You are already registered for this organisation");
            await Task.CompletedTask.ConfigureAwait(false);

        }
     

    }
}