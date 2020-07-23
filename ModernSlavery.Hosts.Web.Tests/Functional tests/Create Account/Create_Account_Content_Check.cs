using Geeks.Pangolin;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class Create_Account_Content_Check : UITest
    {
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

        }
        private bool TestRunFailed = false;
        private string _webAuthority;

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
        [Test, Order(1)]
        public async Task GoToCreateAccountPage()
        {
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            await Task.CompletedTask;

        }
        [Test, Order(2)]
        public async Task Expect_Email_Fields()

        {
            ExpectHeader("Email address");
            BelowHeader("Email address").Expect("Enter an email address that you can access. The service will send you an email to verify your identity.");
            ExpectField("Email address");
            ExpectField("Confirm your email address");

            await Task.CompletedTask;

        }


        [Test, Order(3)]
        public async Task Expect_Your_Details_Fields()

        {
            ExpectHeader("Your details");
            BelowHeader("Your details").Expect("Enter your name and job title.");
            ExpectField("First name");
            ExpectField("Last name");
            ExpectField("Job title");

            await Task.CompletedTask;

        }

        [Test, Order(3)]
        public async Task Expect_Create_Password_Validation_Rules()

        {
            ExpectHeader("Create password");
            Expect(What.Contains, "Must be at least 8 characters long.");
            Expect(What.Contains, "It must also have at least one of ");
            Expect(What.Contains, "each");
            Expect(What.Contains, " of the following:");

            Expect(What.Contains, "lower-case letter");
            Expect(What.Contains, "capital letter and");
            Expect(What.Contains, "number");
          
            Expect(What.Contains, "Your password must be at least 8 characters long.");


            ExpectField("Password");
            ExpectField("Confirm password");

            await Task.CompletedTask;
        }

        [Test, Order(4)]
        public async Task Expect_Create_Account_Terms_And_Conditions()

        {
            Expect("We will only use your contact details to send you information relating to Modern Slavery reporting and, with your consent, for the following purpose.");
            ExpectLabel("I would like to receive information about webinars, events and new guidance");
            ExpectLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            await Task.CompletedTask;
        }

        [Test, Order(4)]
        public async Task Expect_Continue_Creation_Button()
        {
            ExpectButton("Continue");
            await Task.CompletedTask;
        }
    } 
    
}