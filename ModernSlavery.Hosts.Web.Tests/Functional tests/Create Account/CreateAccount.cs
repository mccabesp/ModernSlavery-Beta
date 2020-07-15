using Geeks.Pangolin;
using Geeks.Pangolin.Service.DriverService;
using NUnit.Framework;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace ModernSlavery.Hosts.Web.Tests
{
    public abstract class CreateAccount : UITest
    {
        string _firstname; string _lastname; string _title; string _email; string _password;
        public CreateAccount(string firstname, string lastname, string title, string email, string password) : base()
        {
            _firstname = firstname; _lastname = lastname; _title = title; _email = email; _password = password;
        }

        private string URL;

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

        [Test]
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
            ClickLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            Click("Continue");

            ExpectHeader("Verify your email address");
            await Task.CompletedTask;
        }
        [Test]
        public async Task ExtractVerifyURL()
        {

            Expect(What.Contains, "We have sent you a confirmation email to");
            Expect(What.Contains, _email);
            Expect(What.Contains, "Follow the instructions in the email to continue your sign up.");


            //get email verification link
            URL = WebDriver.FindElement(By.LinkText(_email)).GetAttribute("href");

            await Task.CompletedTask;

        }

        public async Task VerifyEmail()
        {

            //verify email
            Goto(URL);

            Set("Email").To(_email);
            Set("Password").To(_password);

            Click(The.Bottom, "Sign In");
            ExpectHeader("You've confirmed your email address");

            Expect("To complete the registration process for Modern Slavery reporting please continue.");
            Click("Continue");

            ExpectHeader("Privacy Policy");

            Click("Continue");

            ExpectHeader("Select an organisation");

            await Task.CompletedTask;

        }

    }
}