using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class SignInContentCheck : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        public Task AxeHel { get; private set; }

        public SignInContentCheck() : base(_firstname, _lastname, _title, _email, _password)
        {


        }


        [Test, Order(11)]

        public async Task NavigateToSignInPage()
        {

            Click("Sign out");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Expect("Signed out");
            Click(The.Top, "Sign in");

            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(12)]

        public async Task CheckContentOfSignInPage()
        {

            ExpectHeader("Sign in or create an account");
            Expect(What.Contains, "If you have an account, sign in using your email address and password.");
            Expect(What.Contains, "After signing into your account you can register a new organisation or manage your existing organisations.");
            Expect("Email");
            Expect("Password");
            Expect("Sign in");

            Expect(What.Contains, "Problems with your password?");
            ExpectLink("Reset your password");

            ExpectHeader("No account yet?");
            Expect(What.Contains, "If you're new to the service you will need to create an account. This will allow you to register organisations and submit information about their modern slavery statements.");
            Expect("Create an account");
            await Task.CompletedTask.ConfigureAwait(false);

        }
        [Test, Order(14)]

        public async Task BannerCheck()
        {

            Expect("Modern slavery statement registry");
            await Task.CompletedTask.ConfigureAwait(false);

        }


    }
}