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
    public class Fasttrack : UITest
    {
        public Fasttrack() : base(TestRunSetup.WebDriverService)
        {

        }
        private string _webAuthority;
        private IDataRepository _dataRepository;
        private IFileRepository _fileRepository;
        private string URL;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            //Get the url from the test web host
            _webAuthority = TestRunSetup.TestWebHost.GetHostAddress();
            if (Debugger.IsAttached) Debug.WriteLine($"Kestrel authority: {_webAuthority}");
            Console.WriteLine($"Kestrel authority: {_webAuthority}");

            //Get the data repository from the test web host
            _dataRepository = TestRunSetup.TestWebHost.GetDataRepository();

            //Get the file repository from the test web host
            _fileRepository = TestRunSetup.TestWebHost.GetFileRepository();
            if (Debugger.IsAttached) Debug.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");
            Console.WriteLine($"FileRepository root: {_fileRepository.GetFullPath("\\")}");


            //_dataRepository.Insert<User>(new User { EmailAddress = "test@uat.co" });
            //_dataRepository.SaveChangesAsync();
        }
        private bool TestRunFailed = false;

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
            //if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;
            //TeardownTest();
        }

        [Test]
        public void Fastrack_CreateAndVerifyUser(){

            //succesful create account journey 
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            Set("Email address").To(Create_Account.roger_email);
            Set("Confirm your email address").To(Create_Account.roger_email);

            Set("First name").To(Create_Account.roger_first);
            Set("Last name").To(Create_Account.roger_last);
            Set("Job title").To(Create_Account.roger_job_title);

            Set("Password").To(Create_Account.roger_password);
            Set("Confirm password").To(Create_Account.roger_password);

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");

            ExpectHeader("Verify your email address");



            Expect(What.Contains, "We have sent you a confirmation email to");
            Expect(What.Contains, Create_Account.roger_email);
            Expect(What.Contains, "Follow the instructions in the email to continue your sign up.");


            //get email verification link
            var URL = WebDriver.FindElement(By.LinkText(Create_Account.roger_email)).GetAttribute("href");

            Logout();


            //verify roger's email
            Goto(URL);

            Set("Email").To(Create_Account.roger_email);
            Set("Password").To(Create_Account.roger_password);

            Click(The.Bottom, "Sign In");
            ExpectHeader("You've confirmed your email address");

            Expect("To complete the registration process for Modern Slavery reporting please continue.");
            Click("Continue");

            ExpectHeader("Privacy Policy");

            Click("Continue");

            ExpectHeader("Select an organisation");
            Logout();
        }
        [Test, Parallelizable]
        public void Fastrack_Registration_Success()
        {
            Goto("/");
            Click("Sign in");
            Set("EMail").To(Create_Account.roger_email);
            Set("Password").To(Create_Account.roger_password);

            Click(The.Bottom, "Sign in");


            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");

            Click("Continue");

            ExpectHeader("Fast track registration");

            BelowHeader("Fast track registration").ExpectText("If you have received a letter you can enter your employer reference and security code to fast track your organisation`s registration");

            BelowHeader("Fast track registration").ExpectLabel("Employer reference");
            BelowHeader("Fast track registration").ExpectField("Employer reference");

            BelowHeader("Fast track registration").ExpectLabel("Security code");
            BelowHeader("Fast track registration").ExpectField("Security code");

            ExpectButton("Continue");
        }

    }
}