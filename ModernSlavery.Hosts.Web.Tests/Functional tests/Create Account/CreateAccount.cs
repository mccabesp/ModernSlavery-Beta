using Geeks.Pangolin;
using Geeks.Pangolin.Service.DriverService;
using NUnit.Framework;
using System.Threading.Tasks;
using OpenQA.Selenium;
using ModernSlavery.Infrastructure.Hosts;
using System.Diagnostics;
using System;
using ModernSlavery.Testing.Helpers;
using ModernSlavery.Core.Interfaces;
using NUnit.Framework.Interfaces;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public abstract class CreateAccount : UITest
    {
        string _firstname; string _lastname; string _title; string _email; string _password;
        private string _webAuthority;
        private IDataRepository _dataRepository;
        private IFileRepository _fileRepository;
        private string URL;
        public string UniqueEmail;
        public CreateAccount(string firstname, string lastname, string title, string email, string password) : base()
        {
            UniqueEmail = email + DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            _firstname = firstname; _lastname = lastname; _title = title; _email = UniqueEmail; _password = password;


        }

        private bool TestRunFailed = false;

        //[SetUp]
        //public void SetUp()
        //{
        //    if (TestRunFailed)
        //        Assert.Inconclusive("Previous test failed");
        //    else
        //        SetupTest(TestContext.CurrentContext.Test.Name);
        //}




        //[TearDown]
        //public void TearDown()
        //{

        //    if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed) TestRunFailed = true;

        //}
        
        [Test, Order(1)]
        public async Task GoToCreateAccountPage()
        {
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in or create an account");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            await Task.CompletedTask;

        }

        [Test, Order(2)]
        public async Task EnterPersonalDetails()
        {
            Set("Email address").To(_email);
            Set("Confirm your email address").To(_email);

            Set("First name").To(_firstname);
            Set("Last name").To(_lastname);
            Set("Job title").To(_title);

            Set("Password").To(_password);
            Set("Confirm password").To(_password);

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in surveys about modern slavery");

            await Task.CompletedTask;
        }

        [Test, Order(3)]

        public async Task ClickingContinueNavigatesToVerification()
        {
            Click("Continue");
            ExpectHeader("Verify your email address");

            await Task.CompletedTask;
        }


            [Test, Order(4)]
        public async Task ExtractVerifyURL()
        {
           
            Expect(What.Contains, "We have sent a confirmation email to");
            Expect(What.Contains, _email);
            Expect(What.Contains, "Follow the instructions in the email to finish creating your account.");


            //get email verification link
            URL = WebDriver.FindElement(By.LinkText(_email)).GetAttribute("href");

            await Task.CompletedTask;

        }

        [Test, Order(5)]
        public async Task VerifyEmail()
        {

            //verify email
            Goto(URL);

            Set("Email").To(_email);
            Set("Password").To(_password);

            Click(The.Bottom, "Sign In");
            ExpectHeader("You've confirmed your email address");

            Expect("To finish creating your account, select continue.");
            Click("Continue");

            
            ExpectHeader("Select an organisation");

            await Task.CompletedTask;

        }

    }
}