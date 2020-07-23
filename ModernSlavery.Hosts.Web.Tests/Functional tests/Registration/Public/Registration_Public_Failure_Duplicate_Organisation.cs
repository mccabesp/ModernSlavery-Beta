﻿using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Awaiting fix for public registration in 3113")]

    public class Registration_Public_Failure_Duplicate_Organisation : Registration_Public_Success
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;

        [Test, Order(40)]
        public async Task SearchForDuplicateOrg()
        {
            Goto("/");

            await NavigateToOrgPage();
            await SearchForOrg();

            await Task.CompletedTask; 
        }
        [Test, Order(42)]
        public async Task SelectDuplicateOrg ()        
        {
            
            ClickButton(That.Contains, "Choose");

            //org already registered
            //error should appear

            Expect("The following errors were detected");
            Expect("You have already registered this organisation");

            await Task.CompletedTask;

        }
    }
}