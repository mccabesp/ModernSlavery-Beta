using Geeks.Pangolin;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Hosts;
using ModernSlavery.Testing.Helpers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using static ModernSlavery.Core.Extensions.Web;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using ModernSlavery.Core.Entities;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]
    public class Fasttrack : CreateAccount
    {
        const string _firstname = Create_Account.roger_first; const string _lastname = Create_Account.roger_last; const string _title = Create_Account.roger_job_title; const string _email = Create_Account.roger_email; const string _password = Create_Account.roger_password;
        public Fasttrack() : base(_firstname, _lastname, _title, _email, _password)
        {


        }
        
        [Test, Order(20)]
        public void Fastrack_Registration_Success()
        {
            //Goto("/");
            ////Click("Sign in");
            ////Set("EMail").To(Create_Account.roger_email);
            ////Set("Password").To(Create_Account.roger_password);

            ////Click(The.Bottom, "Sign in");


            //Click("Register an organisation");


            //ExpectHeader("Registration Options");

            //ClickLabel("Fast Track");

            //Click("Continue");

            //ExpectHeader("Fast track registration");

            //BelowHeader("Fast track registration").ExpectText("If you have received a letter you can enter your employer reference and security code to fast track your organisation`s registration");

            //BelowHeader("Fast track registration").ExpectLabel("Employer reference");
            //BelowHeader("Fast track registration").ExpectField("Employer reference");

            //BelowHeader("Fast track registration").ExpectLabel("Security code");
            //BelowHeader("Fast track registration").ExpectField("Security code");

            //ExpectButton("Continue");

            Click("Register an organisation");


            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //register organisation
            Set("Employer reference").To(Registration.EmployerReference_Milbrook);
            Set("Security code").To(Registration.SecurtiyCode_Millbrook);

            Click("Continue");


            ExpectHeader("Confirm your organisation’s details");

            //expect organisation details
            AtRow("Organisation name").Expect(Registration.OrgName_Millbrook);
            AtRow("Company number").Expect(Registration.CompanyNumber_Millbrook);
            AtRow("Registered address").Expect(Registration.RegisteredAddress_Millbrook);

            //using contains due to label including encoded spaces and not being detected properly
            AtRow(That.Contains, "Business").Expect(Registration.SicCode_Milbrook.Item1);
            AtRow(That.Contains, "Business").Below(Registration.SicCode_Milbrook.Item1).Expect(Registration.SicCode_Milbrook.Item2);
            AtRow(That.Contains, "Business").RightOf(Registration.SicCode_Milbrook.Item2).Expect(Registration.SicCode_Milbrook.Item3);

            Click("Confirm");
            ExpectHeader("You can now publish a Modern Slavery statement on behalf of this organisation.");

            At("Employer name").Expect(Registration.OrgName_Millbrook);

            Below("Employer name").ExpectText("You can also specify whether this employer is in scope of the reporting regulations.");

            Click("Continue");

            ExpectHeader("Select an organisation");

            ExpectRow(Registration.OrgName_Millbrook);
            AtRow(Registration.OrgName_Millbrook).Column("Organisation Status").Expect("Registration Complete");
        }

    }
}