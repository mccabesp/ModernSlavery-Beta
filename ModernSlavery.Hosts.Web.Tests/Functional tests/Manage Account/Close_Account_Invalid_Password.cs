using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Temporary igore")]
    public class Close_Account_Invalid_Password : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email+"caip"; const string _password = Create_Account.roger_password;
        public Close_Account_Invalid_Password() : base(_firstname, _lastname, _title, _email, _password)
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

        public async Task InvalidDetails_Results_In_Validaiton()
        {
            Set("Enter your password to confirm").To("invalid");
            
            ClickText("Close account");

            ExpectHeader("There is a problem");
            Expect("Could not verify your current password");
            await Task.CompletedTask;

        }

    }
}