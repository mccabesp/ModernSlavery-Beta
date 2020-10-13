using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary igore")]
    public class ManageAccount_Change_Email : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public ManageAccount_Change_Email() : base(_firstname, _lastname, _title, _email, _password)
        {


        }


        [Test, Order(11)]

        public async Task ClickManageAccount_RedirectsToChangeDetailsPage()
        {
            Click(The.Top, "Manage Account");
            await AxeHelper.CheckAccessibilityAsync(this);

            ExpectHeader("Login details");

            await Task.CompletedTask;

        }

        [Test, Order(12)]

        public async Task ClickingChangeEmailLeadsToEmailAddressPage()
        {
            Click(The.Top, "Change");
            await AxeHelper.CheckAccessibilityAsync(this);

            ExpectHeader("Enter your new email address");
            await Task.CompletedTask;

        }

        [Test, Order(13)]

        public async Task ChangeEmailAddress()
        {
            Set("Email address").To(Create_Account.edited_email);
            Set("Confirm your email address").To(Create_Account.edited_email);

            ClickText("Confirm");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");

            ExpectHeader("Your details have been updated successfully");

            Click("Manage Account");

            await AxeHelper.CheckAccessibilityAsync(this);

            ExpectHeader("Manage your account");

            AtRow("Email address").Expect(Create_Account.edited_email);
            await Task.CompletedTask;

        }

    }
}