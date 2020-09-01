﻿using Geeks.Pangolin;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Sumbission_Submit_Report_Mandatory_Fields_Only : Submission_Complete_Mandatory_Sections
    {
        [Test, Order(50)]
        public async Task NavigateToSubmissionPage()
        {
            Click("Manage organisations");
            Click("Interfloor limited");
            Click("Continue");
            ExpectHeader("Review before submitting");
    

            await Task.CompletedTask;
        }

        [Test, Order(52)]
        public async Task EnsureSectionsAreCompleted()
        {
            //mandatory sections should be completed
            RightOfText("Your modern slavery statement").ExpectText("Completed");
            RightOfText("Areas covered by your modern statement").ExpectText("Completed");
            await Task.CompletedTask;
        }

        [Test, Order(54)]
        public async Task EnsureOptionalSectionsAreIncomplete()
        {
            //all other sections incomplete 
           
            RightOfText("Your organisation").ExpectText("Not Completed");
            RightOfText("Policies").ExpectText("Not Completed");
            RightOfText("Supply chain risks and due diligence (part 1)").ExpectText("Not Completed");
            RightOfText("Supply chain risks and due diligence (part 2)").ExpectText("Not Completed");
            RightOfText("Training").ExpectText("Not Completed");
            RightOfText("Monitoring progress").ExpectText("Not Completed");
            await Task.CompletedTask;
        }

        [Test, Order(56)]
        public async Task SubmitFormWithOptionalSections()
        {
            Click("Confirm and submit");

            Expect("You've submitted your Modern Slavery statement for 2019 to 2020");

            Click("Finish and Sign out");


            WaitForNewPage();

            await Task.CompletedTask;

            //todo validate submitted form

        }
    }
}