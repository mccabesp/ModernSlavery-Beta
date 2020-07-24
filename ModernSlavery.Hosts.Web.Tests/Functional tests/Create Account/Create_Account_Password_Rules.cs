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
    public class Create_Account_Password_Rules : UITest
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
        public async Task FillOutAccountCreationIngormation()
        {
            Set("Email address").To("roger@test.co");
            Set("Confirm your email address").To("roger@test.co");

            Set("First name").To("Roger");
            Set("Last Name").To("Reporter");

            Set("Job title").To("Company Reporter");

            Set("Password").To("Test1234!");
            Set("Confirm Password").To("Test1234!");
            await Task.CompletedTask;

        }

        [Test, Order(3)]
        public async Task TermsAndConditionsContinue()
        {
            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");
            await Task.CompletedTask;

        }


        [Test, Order(3)]
        public async Task ValidationForCorrectPassword()
        {
            ExpectHeader("Verify your email address");
            Expect("We have sent a confirmation email to");
            Expect("roger@test.co");
            Expect("Follow the instructions in the email to continue your registration.");
            await Task.CompletedTask;
        }
    }
}