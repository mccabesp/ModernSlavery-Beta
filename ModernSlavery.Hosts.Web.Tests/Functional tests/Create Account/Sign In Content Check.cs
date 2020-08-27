using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class SignInContentCheck : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public SignInContentCheck() : base(_firstname, _lastname, _title, _email, _password)
        {


        }


        [Test, Order(11)]

        public async Task NavigateToSignInPage()
        {
            Click("Sign out");
            Expect("Signed out");
            Click("Continue");
            ExpectHeader("Submit a modern slavery statement");
            Click("Start now");
            ExpectHeader("Sign in or create an account");
            await Task.CompletedTask;

        }

        [Test, Order(12)]

        public async Task CheckContentOfSignInPage()
        {
            ExpectHeader("Sign in or create an account");
            Expect("If you have an account please sign in using your email address and password");
            Expect("Email");
            Expect("Password");
            Expect("Sign in");

            Expect("Problems with your passoword?");
            Expect("Reset your password");

            ExpectHeader("No account yet?");
            Expect("If you`re new to the service you will need to create an account. This will allow you to register organisations and submit information about their modern slavery statements");
            Expect("Create an account");
            await Task.CompletedTask;

        }

    }
}