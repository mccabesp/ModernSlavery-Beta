﻿using Geeks.Pangolin;
using Geeks.Pangolin.Core.Helper;
using Geeks.Pangolin.Helper.UIContext;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture, Ignore("Need to revie text boxes")]


    public class Submission_Areas_Content_Check : Private_Registration_Success
    {
        [Test, Order(40)]
        public async Task StartSubmission()
        {

            RefreshPage();

            ExpectHeader("Select an organisation");

            Click(org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this);
            SubmissionHelper.MoreInformationRequiredComplete(this, true, OrgName: org.OrganisationName);

            ExpectHeader(That.Contains, "Manage your modern slavery statement submissions");

            Click(The.Bottom, "Start Draft");


            ExpectHeader("Before you start");
            Click("Start now");
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ModernSlavery.Testing.Helpers.Extensions.SubmissionHelper.GroupOrSingleScreenComplete(this, OrgName: org.OrganisationName);
            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST");
            ExpectHeader("Your modern slavery statement");
            await Task.CompletedTask;
        }

        [Test, Order(42)]
        public async Task NavigateToAreasPage()
        {
            ExpectHeader("Your modern slavery statement");


            Click("Continue");
            await AxeHelper.CheckAccessibilityAsync(this);
            ExpectHeader("Areas covered by your modern slavery statement");

            await Task.CompletedTask;
        
        }
        [Test, Order(44)]
        public async Task VerifyAreaPageContent()
        {
            ExpectText("Does your modern slavery statement cover the following areas in relation to slavery and human trafficking?");
            ExpectText("If you select 'no', please tell us why this area is not covered.");

            for (int i = 0; i < (Submission.AreaHeaders.Length -1); i++)
            {
                AreaContentCheck(this, Submission.AreaHeaders[i], Submission.AreaHeaders[i + 1]);
            }

            ExpectButton("Save and continue");
            ExpectButton("Cancel");
            await Task.CompletedTask;

        }

        private static void AreaContentCheck(UIContext ui, string SectionHeader, string NextSectionHeader = null)         
        {
            ui.ExpectHeader(SectionHeader);

            if (NextSectionHeader.HasValue())
            {
                ui.BelowHeader(SectionHeader).AboveHeader(NextSectionHeader).ExpectLabel("Yes");
                ui.BelowHeader(SectionHeader).AboveHeader(NextSectionHeader).ExpectLabel("No");

                ui.BelowHeader(SectionHeader).AboveHeader(NextSectionHeader).ClickLabel("No");
                ui.BelowHeader(SectionHeader).AboveHeader(NextSectionHeader).ExpectField("Please provide details");
                ui.BelowHeader(SectionHeader).AboveHeader(NextSectionHeader).ClickLabel("Yes");
                ui.BelowHeader(SectionHeader).AboveHeader(NextSectionHeader).ExpectNoField("Please provide details");
            }
            else
            {
                ui.BelowHeader(SectionHeader).ExpectLabel("Yes");
                ui.BelowHeader(SectionHeader).ExpectLabel("No");
                ui.BelowHeader(SectionHeader).ClickLabel("No");
                ui.BelowHeader(SectionHeader).ExpectField("Please provide details");
                ui.BelowHeader(SectionHeader).ClickLabel("Yes");
                ui.BelowHeader(SectionHeader).ExpectNoField("Please provide details");
            }
        }
    }
}