using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class CloseAccount : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public CloseAccount() : base(_firstname, _lastname, _title, _email, _password)
        {
        }

        [Test, Order(11)]
        public async Task ClickManageAccount_RedirectsToChangeDetailsPage()
        {
            Click(The.Top, "Your details");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Manage your account");

            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(12)]
        public async Task ChangeDetailsPage_ClickCloseYouAccount_GoestoClosePage()
        {
            ClickLink("Close your account");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Close your account");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(13)]
        public async Task ClickingCloseAccountClosesAccount()
        {
            Expect("Closing your account will not affect any information you have published on this service.");
            Expect("Other users with accounts for any of your registered organisations will still be able to submit and edit information.");

            Expect("Once you have closed your account:");
            Expect("you will not be able to submit information on the service");
            Expect("you will no longer receive communications from us");

            //no registered orgs
            ExpectNo("Closing your account will leave one or more of your registered organisations with no one to submit a modern slavery statement on their behalf. It can take up to a week to register an organisation");

            Below("Are you sure you want to close your account?").Set("Enter your password to confirm").To(Create_Account.roger_password);

            ClickText("Close account");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            Expect("Your account has been closed");
            Expect("You are now signed out of the Modern slavery statement registry.");
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}