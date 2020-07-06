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

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    [Parallelizable]
    public class CreateAccount: UITest
    {
        public CreateAccount():base(TestRunSetup.WebDriverService)
        {
            
        }

        private string _webAuthority;
        private IDataRepository _dataRepository;
        private IFileRepository _fileRepository;

        private bool TestRunFailed=false;

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

        [Test, Parallelizable]
        public void Create_Account_ContentCheck()
        {
            TestContext.Out.WriteLine($"Kestrel authority: {_webAuthority}");

            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");


            ExpectHeader("Create an Account");
            ExpectHeader("Email address");
            BelowHeader("Email address").Expect("Enter an email address that you can access. The service will send you an email to verify your identity.");

            ExpectField("Email address");
            ExpectField("Confirm your email address");

            ExpectHeader("Your details");
            BelowHeader("Your details").Expect("Enter your name and job title.");

            ExpectField("First name");
            ExpectField("Last name");
            ExpectField("Job title");

            ExpectHeader("Create password");
            Expect(What.Contains, "Must be at least 8 characters long.");
            Expect(What.Contains, "It must also have at least one of ");
            Expect(What.Contains, "each");
            Expect(What.Contains, "of the following:");
            Expect(What.Contains, "capital letter and");
            Expect(What.Contains, "number");
            Expect(What.Contains, "lower-case letter");
            Expect(What.Contains, "Must be at least 8 characters long.");

            ExpectField("Password");
            ExpectField("Confirm password");

            Expect("We will only use your contact details to send you information relating to Modern Slavery reporting and, with your consent, for the following purpose.");
            ExpectLabel("I would like to receive information about webinars, events and new guidance");
            ExpectLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            ExpectButton("Continue");
        }
        [Test, Order(2)]
        public void Create_Account_Success()
        {
            TestContext.Out.WriteLine($"Kestrel authority: {_webAuthority}");

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

            Expect("We have sent a confirmation email to");
            Expect("roger@uat.co");
            Expect("Follow the instructions in the email to continue your registration");
        }

        [Test, Order(3) ]
        public void Create_Account_Awaiting_Verification_Check()
        {
            //if creating account for email address used in last 24 hours validation should appear

            //register account with email roger@test.co again
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            Set("Email address").To("roger@uat.co");
            Set("Confirm your email address").To("roger@uat.co");

            Set("First name").To("Roger");
            Set("Last name").To("Reporter");
            Set("Job title").To("Tester");

            Set("Password").To("Test1234");
            Set("Confirm password").To("Test1234");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");

            Expect("The following errors were detected");
            Expect("This email address is awaiting confirmation. Please enter a different email address or try again in 23 hours");
        }


        [Test, Order(5)]
        public void Create_Account_Failure_Existing_User()
        {
            //roger already registered in the system
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            Set("Email address").To(Create_Account.existing_email);
            Set("Confirm your email address").To(Create_Account.existing_email);

            Set("First name").To("Existing");
            Set("Last name").To("User");
            Set("Job title").To("Reporter");

            Set("Password").To("Test1234");
            Set("Confirm password").To("Test1234");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");


            //todo check validation messages
            Expect("The following errors were detected");
            Expect("This email address has already been registered. Please enter a different email address or request a password reset.");
        }

        [Test, Parallelizable]
        public void Create_Account_Password_Rules()
        {
            //roger already registered in the system
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            Set("Email address").To(Create_Account.existing_email);
            Set("Confirm your email address").To(Create_Account.existing_email);

            Set("First name").To("Existing");
            Set("Last name").To("User");
            Set("Job title").To("Reporter");

            Set("Password").To("Test1234");
            Set("Confirm password").To("Test1234");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");


            //todo check validation messages
            Expect("The following errors were detected");
            Expect("This email address has already been registered. Please enter a different email address or request a password reset.");
        }
        [Test, Parallelizable]
        public void Create_Account_Validation_Check()
        {
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");

            ExpectHeader("Create an Account");

            //invalid email address
            Set("Email address").To("invalid");
            Click("Continue");
            //Expect("There`s a problem with your email address");

            //Expect("Please include an '@' in the email address. 'invalid' is missing an '@'.");
            Expect(What.Contains, "Please include");

            //different email addresses
            Set("Email address").To("test@test.test");
            Set("Confirm email address").To("test2@test.test");
            Click("Continue");

            Expect("The following errors were detected");
            Expect("The email address and confirmation do not match");

            Set("Confirm email address").To("test@test.test");

            Click("Continue");

            //personal details
            //leave blank to test validaition
            //todo check validation messages
            Expect("The following errors were detected");
            Expect("You need to provide a first name");
            Expect("You need to provide a last name");
            Expect("You need to provide a job title");
        }
    }
}