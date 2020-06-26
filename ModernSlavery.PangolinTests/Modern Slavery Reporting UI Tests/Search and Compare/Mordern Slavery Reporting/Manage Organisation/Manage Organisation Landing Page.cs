using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class ManageOrganisationLandingPage : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //log in as organisation manager 

            ExpectHeader("Select an organisation");
            Expect("Submit or view a modern slavery statement");

            Expect("Once you've selected an organisation you can:");
            Expect("State whether this organisation is required to publish an annual modern slavery statement");
            Expect("Provide a link to the full modern slavery statement published on the organisation's website");
            Expect("Enter information about this organisation's modern slavery statement");
            Expect("Save this information as a draft, and complete it at a later date before submitting it");
            Expect("The information you submit will be published as a Modern slavery statement report in the 'Find and view a modern slavery statement' service.");

           
        }
    }
}