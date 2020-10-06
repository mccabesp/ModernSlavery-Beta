using NUnit.Framework;
using Geeks.Pangolin;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using ModernSlavery.Testing.Helpers.Classes;
using ModernSlavery.Hosts.Web.Tests;

namespace ModernSlavery.Hosts.Web.GoogleTests
{
    [TestFixture]
    [Parallelizable]
    public class WebHostSampleTestGoogle: BaseUITest
    {
        public WebHostSampleTestGoogle() : base(TestRunSetup.TestWebHost, TestRunSetup.WebDriverService)
        {

        }

        [Test]
        public void WebTestHost_Search1()
        {
            //Type("ModernSlavery");
            //Press(Keys.Enter);
            SetCSS("#tsf > div:nth-child(2) > div.A8SBwf > div.RNNXgb > div > div.a4bIc > input").To("ModernSlavery");
            ClickCSS("#tsf > div:nth-child(2) > div.A8SBwf > div.FPdoLc.tfB0Bf > center > input.gNO89b");
        }

        [Test]
        public void WebTestHost_Search2()
        {
            Goto("/");
            //Type("ModernSlavery");
            //Press(Keys.Enter);
            SetCSS("#tsf > div:nth-child(2) > div.A8SBwf > div.RNNXgb > div > div.a4bIc > input").To("ModernSlavery");
            ClickCSS("#tsf > div:nth-child(2) > div.A8SBwf > div.FPdoLc.tfB0Bf > center > input.gNO89b");

            Assert.Throws<System.AggregateException>(() =>
            {
                /// This is an example of how to catch multple exceptions in a single aggregate exception

                Try(() => { ExpectHeader("This not found"); },
                    () => { ExpectHeader("And this not found"); },
                    () => { ExpectHeader("And even this not found"); });

                /*The above return details as follows

                Message: 
                System.AggregateException : One or more errors occurred. (Found no This not found) (Found no And this not found) (Found no And even this not found)
                  ----> Geeks.Pangolin.Core.Exception.CommandException : Found no This not found
                  ----> Geeks.Pangolin.Core.Exception.CommandException : Found no And this not found
                  ----> Geeks.Pangolin.Core.Exception.CommandException : Found no And even this not found
                 */

            });
            
        }


    }
}
 