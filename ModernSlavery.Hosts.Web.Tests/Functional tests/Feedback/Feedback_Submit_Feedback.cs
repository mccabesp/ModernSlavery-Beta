﻿using Geeks.Pangolin;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Feedback_Submit_Feedback : Private_Registration_Success
    {
           [Test, Order(40)]
        public async Task NavigateToFeedbackPage()
        {
            Click("feedback");
            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            ExpectHeader("Send us feedback");
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(42)]
        public async Task What_Did_You_Do()
        {
            Below("What did you do on this service?").ClickLabel("Submitted a statement");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task Easy_Or_Difficult()
        {
            Below("How easy or difficult was it to use the service?").ClickLabel("Easy");
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(46)]
        public async Task How_Can_We_Improve()
        {
            BelowField("How can we improve the service?").Expect("You have 2000 characters remaining");

            Set("How can we improve the service?").To("Keep on keeping on");

            BelowField("How can we improve the service?").Expect("You have 1982 characters remaining");
            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(47)]
        public async Task Email_Phone()
        {
            BelowLabel("Your email address (optional)").Set(The.Top).To("roger@uat.co");
            BelowLabel("Your phone number (optional)").Set(The.Top).To("1234");
            


            await Task.CompletedTask.ConfigureAwait(false);
        }
        [Test, Order(48)]
        public async Task Submit_Feedback()
        {
            Click("Submit");

            await AxeHelper.CheckAccessibilityAsync(this, httpMethod: "POST").ConfigureAwait(false);

            ExpectHeader("Thank you");

            Expect("Thank you for giving us your feedback about this service. We regularly update the service and pay careful attention to all the comments we receive.");


            await Task.CompletedTask.ConfigureAwait(false);
        }
        
        [Test, Order(50)]
        public async Task Return_To_Manage_Orgs()
        {
            ClickText(That.Contains, "Return to manage organisations");

            await AxeHelper.CheckAccessibilityAsync(this).ConfigureAwait(false);

            Expect("Your registered organisations");
            await Task.CompletedTask.ConfigureAwait(false);
        }

    }
}