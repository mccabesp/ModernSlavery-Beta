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
using ModernSlavery.Testing.Helpers.Classes;

namespace ModernSlavery.Hosts.Web.Tests
{
    
    [TestFixture]

    
    public class MVP1_Start_Page : BaseUITest
    {
        public MVP1_Start_Page() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {

        }
        private bool TestRunFailed;
        [SetUp]
        public void SetUp()
        {
            if (TestRunFailed)
                Assert.Inconclusive("Previous test failed");
            else
                SetupTest(TestContext.CurrentContext.Test.Name);
        }




        [TearDown]
        public void TearDown()
        {

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;

        }


        //test case for checking the temporary mocked gds start page
        [Test, Order(20)]
        public async Task NavigateToStartPage()
        {
            Goto("/start");
            ExpectHeader("Submit a modern slavery statement");
            await Task.CompletedTask;
        }


        [Test, Order(22)]
        public async Task CheckStartPageContent()
        {
            Try(
                 () => Expect("We have recently launched a government-run registry of modern slavery statements published in the UK. Use this service to:"),
            () => { Expect("provide us with a link to the modern slavery statement published on your organisation’s website"); },
            () => { Expect("answer some questions about your statement"); },
            () => { ExpectHeader("What we do with your information"); },
            () => { Expect("We will use the information you provide us with to publish a summary of your statement on our View a modern slavery statement service. This will include a link to the full statement on your website."); },
            () => { ExpectHeader("Who can use this service"); },
            () => { Expect("Any organisation that has created a modern slavery statement can use this service, including those that:"); },
            () => { Expect("are legally required to publish an annual modern slavery statement in the UK"); },
            () => { Expect("answer some questions about your statement"); },
            () => { Expect("have chosen to publish a modern slavery statement voluntarily"); }) ;
            await Task.CompletedTask;
        }

        [Test, Order(24)]
        public async Task StartButtonLeadsToSignInPage()
        {
            ClickText("Start now");
            ExpectHeader("Sign in or create an account");
            await Task.CompletedTask;
        }
    }
}