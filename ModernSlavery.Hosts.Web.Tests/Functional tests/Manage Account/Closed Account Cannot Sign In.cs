using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class ClosedAccountCannotSignIn : CloseAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

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
        Click("Sign in");
        Set("Email").To(Create_Account.roger_email);
        Set("Password").To(Create_Account.roger_password);
        await Task.CompletedTask;
    }
        [Test, Order(24)]

        public async Task ClickingSignInLeadsToError()
        {

            Click(The.Bottom, "Sign in");
            Expect("There`s a problem with your email address or password");
            await Task.CompletedTask;

        }



    }
}