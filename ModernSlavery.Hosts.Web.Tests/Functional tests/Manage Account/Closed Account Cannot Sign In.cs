using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary igore")]
    public class ClosedAccountCannotSignIn : CloseAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email+"cacsi"; const string _password = Create_Account.roger_password;

        [Test, Order(20)]

        public async Task Logout()
        {
            Logout();

            Goto("/");
            await Task.CompletedTask;
        }
        [Test, Order(22)]

        public async Task FillInUserNameAndPassword() { 
            Goto("/");

            await AxeHelper.CheckAccessibilityAsync(this);

            Click("Sign in");

            await AxeHelper.CheckAccessibilityAsync(this);

            Set("Email").To(Create_Account.roger_email);
        Set("Password").To(Create_Account.roger_password);
        await Task.CompletedTask;
    }
        [Test, Order(24)]

        public async Task ClickingSignInLeadsToError()
        {

            Click(The.Bottom, "Sign in");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            Expect("There`s a problem with your email address or password");
            await Task.CompletedTask;

        }



    }
}