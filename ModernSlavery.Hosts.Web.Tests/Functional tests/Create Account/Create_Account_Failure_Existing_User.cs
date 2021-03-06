﻿using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    public class Create_Account_Failure_Existing_User : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public Create_Account_Failure_Existing_User() : base(_firstname, _lastname, _title, _email, _password)
        {


        }


        [Test, Order(11)]

        public async Task NavigateToCreateAccountPage()
        {

            await GoToCreateAccountPage().ConfigureAwait(false);
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

        }

        [Test, Order(12)]
        public async Task EnterDuplicateDetails()
        {
            await EnterPersonalDetails().ConfigureAwait(false);
            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);


            await Task.CompletedTask.ConfigureAwait(false);

        }

        [Test, Order(13)]
        public async Task ExpectValidationOfExistingUser()
        {
            Expect("The following errors were detected");
            Expect("There's a problem with your registration");

            Expect("This email address has already been registered. Please sign in or enter a different email address");
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}


