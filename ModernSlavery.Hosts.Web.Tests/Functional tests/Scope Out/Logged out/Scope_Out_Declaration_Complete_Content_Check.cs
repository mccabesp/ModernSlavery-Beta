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



    public class Scope_Out_Declaration_Complete_Content_Check : Scope_Out_Mark_Org_As_OOS_LoggedOut
    {

        [Test, Order(50)]
        public async Task VerifyInfo()
        {
          
                ExpectHeader("Declaration complete");


                Expect("We've sent you a confirmation email. We will contact you if we need more information.");

                ExpectHeader("Publishing a statement voluntarily");
                Expect("If you are not legally required to publish a modern slavery statement on your website, you can still create a statement voluntarily and submit it to our service.");
                Expect(What.Contains, "To submit a modern slavery statement to our service, ");
                ExpectLink(That.Contains, "create an account");
                Expect(What.Contains, " and register your organisation.");

                ExpectHeader("Tell us about another organisation’s publishing requirements");
                Expect("If you need to tell us whether another organisation is required to publish a modern slavery statement, click on the ‘start again’ button.");

                Expect("Start again");
                await Task.CompletedTask;
            }


       
    }
}