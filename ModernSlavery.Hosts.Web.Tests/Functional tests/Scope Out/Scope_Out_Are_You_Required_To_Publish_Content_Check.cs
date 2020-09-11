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

namespace ModernSlavery.Hosts.Web.Tests
{

    [TestFixture]

    public class Scope_Out_Are_You_Required_To_Publish_Content_Check : UITest
    {
        private string EmployerReference;

        

        [Test, Order(22)]
        public async Task EnterScopeURLLeadsToOrgIdentityPage()
        {
            Goto(TestData.ScopeUrl);
            ExpectHeader("Are you legally required to publish a modern slavery statement on your website?");
            await Task.CompletedTask;
        }

        


        [Test, Order(28)]
        public async Task CheckPageContent()
        {
            Expect("We sent you a letter telling you we think your organisation is legally required to publish an annual modern slavery statement on your website. If this is not correct please let us know so we can update our records.");

            Expect("Enter the organisation reference and security code provided in the letter we sent you. We will then ask you to confirm your organisation is not required to publish a modern slavery statement and give a reason.");

            ExpectField("Organisation reference");

            ExpectField("Security code");

            ExpectLink("Guidance");
            Expect(What.Contains, "is available to help you work out whether your organisation is required to publish a modern slavery statement.");

            ExpectButton("Continue");
            await Task.CompletedTask;
        }




    }
}