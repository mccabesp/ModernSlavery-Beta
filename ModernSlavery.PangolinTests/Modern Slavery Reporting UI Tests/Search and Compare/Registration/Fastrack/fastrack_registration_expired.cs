﻿using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class Fastrack_Registration_Expired : UITest
    {
        [TestProperty("Work item", "3")]
        [TestCategory("Fasttrack")]
        [TestMethod]
        public override void RunTest()
        {
            LoginAs<RogerReporter>();

            Click("Register an organisation");

            ExpectHeader("Registration Options");

            ClickLabel("Fast Track");
            Click("Continue");

            ExpectHeader("Fast track registration");

            //validation message to appear with expired security code
            Set("Employer reference").To(Registration.ExpiredEmployerReference);
            Set("Security code").To(Registration.ExpiredSecurityCode);

            Click("Continue");

            Expect("Your Security Code has expired");
        }
    }
}