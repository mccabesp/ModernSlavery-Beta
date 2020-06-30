using NUnit.Framework;
using Geeks.Pangolin;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;

namespace ModernSlavery.Hosts.Web.GoogleTests
{
    [TestFixture]
    [Parallelizable]
    public class WebHostSampleTestGoogle: UITest
    {
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

            ExpectHeader("This should not be there");

            //This is an example of how to catch multple exceptions in a single aggregate exception
            Try(() => { ExpectHeader("This not found"); },
                () => { ExpectHeader("And this not found"); },
                () => { ExpectHeader("And even this not found"); });
            
            //TODO The above doesnt return any details as follows
            /*
             Message: 
            System.AggregateException : One or more errors occurred. (Found no This not found) (Found no And this not found) (Found no And even this not found)
              ----> Geeks.Pangolin.Core.Exception.CommandException : Found no This not found
              ----> Geeks.Pangolin.Core.Exception.CommandException : Found no And this not found
              ----> Geeks.Pangolin.Core.Exception.CommandException : Found no And even this not found
             */
        }


    }
}
 