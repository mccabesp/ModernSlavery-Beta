﻿using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Create_Account_Content_Check : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            Goto("/");

            Click("Sign in");

            ExpectHeader("Sign in");

            BelowHeader("No account yet?");
            Click("Create an account");


            ExpectHeader("Create an Account");
            ExpectHeader("Email address");
            BelowHeader("Email address").Expect("Enter an email address that you can access. The service will send you an email to verify your identity.");

            ExpectField("Email address");
            ExpectField("Confirm your email address");

            ExpectHeader("Your details");
            BelowHeader("Your details").Expect("Enter your name and job title.");

            ExpectField("First name");
            ExpectField("Last name");
            ExpectField("Job title");

            ExpectHeader("Create password");
            Expect(What.Contains, "Must be at least 8 characters long.");
            Expect(What.Contains, "It must also have at least one of each of the following:");
            Expect(What.Contains, "lower-case letter and");
            Expect(What.Contains, "number");
            Expect(What.Contains, "lower-case letter");
            Expect(What.Contains, "Must be at least 8 characters long.");

            ExpectField("Password");
            ExpectField("Confirm password");

            Expect("We will only use your contact details to send you information relating to Modern Slavery reporting and, with your consent, for the following purpose.");
            ExpectLabel("I would like to receive information about webinars, events and new guidance");
            ExpectLabel("I'm happy to be contacted for feedback on this service and take part in Modern Slavery surveys");

            ExpectButton("Continue");


        }
    }
}