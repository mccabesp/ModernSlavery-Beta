using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class CloseAccount : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public CloseAccount() : base(_firstname, _lastname, _title, _email, _password)
        {


        }


        [Test, Order(11)]

        public async Task ClickManageAccount_RedirectsToChangeDetailsPage()
        {
            Click(The.Top, "Manage Account");

            ExpectHeader("Login details");

            await Task.CompletedTask;

        }

        [Test, Order(12)]

        public async Task ChangeDetailsPage_ClickCloseYouAccount_GoestoClosePage()
        {
            Click("Close your account");

            ExpectHeader("Close your account");
            await Task.CompletedTask;

        }

        [Test, Order(13)]

        public async Task ClickingCloseAccountClosesAccount()
        {
            Expect("This will not impact any reports already published on the service. Other people registered to the organisation will still be able to submit and change reports.");

            Expect("You will not be able to report for any organisations you have registered");
            Expect("You will not receive communications from the Modern Slavery Reporting service");

            //only user registered for org
            Expect("Closing your account will leave one or more of your registered organisations with no one to submit on their behalf. It can take up to a week to register an organisation");

            Set("Password").To(Create_Account.roger_password);

            Click("Close your account");

            Expect("Your account has been closed");
            Expect("You are now signed out of the Modern Slavery reporting service");
            await Task.CompletedTask;

        }

    }
}