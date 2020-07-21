using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class ManageAccount_Details_Check : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public ManageAccount_Details_Check() : base(_firstname, _lastname, _title, _email, _password)
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

        public async Task ExpectDetails()
        {

            //Try(
            //     () => AtRow("Email address").Expect(Create_Account.roger_email),
            //() => { AtRow("Password").Expect("********"); },
            //() => {ExpectHeader("Personal details"); },
            //() => {AtRow("First name").Expect(Create_Account.roger_first + 1); },
            //() => {AtRow("Last name").Expect(Create_Account.roger_last + 2); },
            //() => {AtRow("Job title").Expect(Create_Account.roger_job_title + 3); },
            //() => {AtRow("Phone number").Expect(""); });


            AtText("Email address").Expect(Create_Account.roger_email);
            AtText("Password").Expect("***********");

            ExpectHeader("Personal details");
            AtText("First name").Expect(Create_Account.roger_first);
            AtText("Last name").Expect(Create_Account.roger_last);
            AtText("Job title").Expect(Create_Account.roger_job_title);

            //todo find out about phone number field
            //AtText("Phone number").Expect("");
            await Task.CompletedTask;

        }


    }

}
