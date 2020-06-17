using Pangolin;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Modern_Slavery_Reporting_UI_Tests
{
    [TestClass]
    public class PoliciesFieldsAndOtherFunctionalityChecks : UITest
    {
        [TestMethod]
        public override void RunTest()
        {
            //Preconditions to be added later

            //Assume precondition navigate through form to Policies page

            ExpectHeader("Policies");

            //Check the fiels on this page are voluntary
            Click("Save and continue");
            ExpectHeader("Due diligence");
            Click("Back");
            ExpectHeader("Policies");

            //Check text box doesn't show until other is selected and then shows
            ExpectNo("Please provide detail");
            Check("Other");
            Expect("Please provide detail");

            //Check once other has been selected it is mandatory to exlpain in text box
            Click("Save and continue");
            Expect("Please provide detail on 'other'");
            ExpectNoHeader("Due diligence");

            //Check you are allowed to continue now this field has been filled out
            Set("Please provide detail").To("This is another reason that does not align with the ones above.");
            Click("Save and continue");
            ExpectNo("Please provide detail on 'other'");
            ExpectHeader("Due diligence");

            //Navigate back to policies page
            Click("Back");
            ExpectHeader("Policies");
            ExpectNoHeader("Due diligence");

            //Check text box is hidden on other deselection
            UnCheck("Other");
            ExpectNo("Please provide detail");
            Check("Freedom of movement");
            Check("Prohibits child labour");
            Check("Victims of modern slavery");
            Click("Save and continue");
            ExpectNo("Please provide detail on 'other'");
            ExpectHeader("Due diligence");



        }
    }
}