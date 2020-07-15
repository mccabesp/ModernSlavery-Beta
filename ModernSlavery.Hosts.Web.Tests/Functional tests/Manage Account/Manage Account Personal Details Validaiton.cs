using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class Manage_Account_Personal_Details_Validation : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public Manage_Account_Personal_Details_Validation() : base(_firstname, _lastname, _title, _email, _password)
        {


        }


        [Test]
        public async Task ClickManageAccount_RedirectsToChangeDetailsPage()
        {
            Click("Manage Account");

            ExpectHeader("Login details");

            await Task.CompletedTask;

        }

        [Test]
        public async Task ChangeDetailsPage_ClickChange_GoestoPersonalDetails()
        {
            //Assign



            //Act
            Click(The.Bottom, "Change");

            //Asert
            ExpectHeader("Change your personal details");

            AtField("First name").Expect(_firstname);
            AtField("Last name").Expect(_lastname);
            AtField("Job title").Expect(_title);
            await Task.CompletedTask;

        }

        [Test]
        public async Task EmptyPersonalDetails_ClickContinue_ShowsErrors()
        {
            //Arrange
            ClearField("First name");
            ClearField("Last name");
            ClearField("Job title");

            Click("Continue");

            //Try(
            //    () => Expect("The following errors were detected"),
            //    () => { Expect("The following errors were detected"); },
            //    () => { Expect("The following errors were detected")}
            //);

            Expect("The following errors were detected");
            Expect("You need to enter your first name");
            Expect("You need to enter your last name");
            Expect("You need to enter your job title");
            await Task.CompletedTask;

        }

    }
}