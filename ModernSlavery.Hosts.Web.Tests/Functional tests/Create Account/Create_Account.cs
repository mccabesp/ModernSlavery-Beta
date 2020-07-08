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

            

            Expect(What.Contains,"We have sent you a confirmation email to");
            Expect(What.Contains, Create_Account.roger_email);
            Expect(What.Contains, "Follow the instructions in the email to continue your sign up.");


            //get email verification link
            URL = WebDriver.FindElement(By.LinkText(Create_Account.roger_email)).GetAttribute("href");
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
            Expect("There's a problem with your registration");
            Expect("This email address is awaiting confirmation. Please enter a different email address or try again in 23 hours and 59 minutes");
        }


        [Test, Order(5)]
        public void Create_Account_Verify_Email()
        {
            
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
        [Test, Order(6)]
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
           // Expect("This email address has already been registered. Please enter a different email address or request a password reset.");

            Expect("This email address has already been registered. Please sign in or enter a different email address");

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

            var time = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds.ToString();

            //use time to ensure email unique
            Set("Email address").To("test@uat.co" +time);
            Set("Confirm your email address").To("test@uat.co" + time);

            Set("First name").To("Existing");
            Set("Last name").To("User");
            Set("Job title").To("Reporter");

            ClickLabel("I would like to receive information about webinars, events and new guidance");
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            //password must be 8 characters
            Set("Password").To("test");
            Set("Confirm password").To("test");           

            Click("Continue");

            Expect("The following errors were detected");
            AtLabel("Password").Expect("The Password must be at least 8 characters long.");
            
            //must contain upper lower digit
            Set("Password").To("testtest");
            Set("Confirm password").To("testtest");

            Click("Continue");

            
            Expect("The following errors were detected");
            AtLabel("Password").Expect("Password must contain at least one upper case, 1 lower case character and 1 digit");

            //must contain upper digit
            Set("Password").To("testtest");
            Set("Confirm password").To("testtest");

            Click("Continue");


            Expect("The following errors were detected");
            AtLabel("Password").Expect("Password must contain at least one upper case, 1 lower case character and 1 digit");

            //must contain lower digit
            Set("Password").To("TESTTEST");
            Set("Confirm password").To("TESTTEST");

            Click("Continue");


            Expect("The following errors were detected");
            AtLabel("Password").Expect("Password must contain at least one upper case, 1 lower case character and 1 digit");

            //must contain digit
            Set("Password").To("Testtest");
            Set("Confirm password").To("Testtest");

            Click("Continue");


            Expect("The following errors were detected");
            AtLabel("Password").Expect("Password must contain at least one upper case, 1 lower case character and 1 digit");

            //passwords must match
            Set("Password").To("Test1234");
            Set("Confirm password").To("Test2345");

            Click("Continue");


            Expect("The following errors were detected");
            AtLabel("Confirm password").Expect("The password and confirmation password do not match.");
        }
        //[Test, Parallelizable]
        //public void Create_Account_Validation_Check()
        //{
        //    Goto("/");

        //    Click("Sign in");

        //    ExpectHeader("Sign in");

        //    BelowHeader("No account yet?");
        //    Click("Create an account");

        //    ExpectHeader("Create an Account");

        //    //invalid email address
        //    Set("Email address").To("invalid");
        //    Click("Continue");
        //    //Expect("There`s a problem with your email address");

        //    //Expect("Please include an '@' in the email address. 'invalid' is missing an '@'.");
        //    Expect(What.Contains, "Please include");

        //    //different email addresses
        //    Set("Email address").To("test@test.test");
        //    Set("Confirm email address").To("test2@test.test");
        //    Click("Continue");

        //    Expect("The following errors were detected");
        //    Expect("The email address and confirmation do not match");

        //    Set("Confirm email address").To("test@test.test");

        //    Click("Continue");

        //    //personal details
        //    //leave blank to test validaition
        //    //todo check validation messages
        //    Expect("The following errors were detected");
        //    Expect("You need to provide a first name");
        //    Expect("You need to provide a last name");
        //    Expect("You need to provide a job title");
        //}
    }
}