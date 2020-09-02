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


            RightOfText("Email address").Expect(What.Contains, Create_Account.roger_email);
            RightOfText("Password").Expect("***********");
            ExpectHeader("Personal details");
            RightOfText("First name").Expect(Create_Account.roger_first);
            RightOfText("Last name").Expect(Create_Account.roger_last);
            RightOfText("Job title").Expect(Create_Account.roger_job_title);

            //todo find out about phone number field
            //AtText("Phone number").Expect("");

            ExpectHeader("Contact preferences");
            RightOf("I would like to receive information about webinars, events and new guidance").Expect("Yes") ;
            RightOf("I'm happy to be contacted for feedback on this service and take part in surveys about modern slavery").Expect("Yes");

            Expect("Help us improve this service");
            Expect(What.Contains, "We want to understand what our users want so that we can");
            Expect(What.Contains, "create a better service.");
            Expect(What.Contains, "Take part in our survey and make your voice heard.");

            Expect("Complete our Survey");
            await Task.CompletedTask;

        }


    }

}
