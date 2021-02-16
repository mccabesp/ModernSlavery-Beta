using Geeks.Pangolin;
using ModernSlavery.Testing.Helpers.Extensions;
using NUnit.Framework;
using System.Threading.Tasks;

namespace ModernSlavery.Hosts.Web.Tests
{
    [TestFixture]

    public class Feedback_Content_Check : Private_Registration_Success
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
            Try(() => { Below("What did you do on this service?").ExpectLabel("Submitted a statement"); },
                    () => { Below("What did you do on this service?").ExpectLabel("Viewed 1 or more statements"); },
                    () => { Below("What did you do on this service?").ExpectLabel("Submitted and viewed statements"); });
            await Task.CompletedTask.ConfigureAwait(false);
        }

        [Test, Order(44)]
        public async Task Easy_Or_Difficult()
        {
            Try(() => { Below("How easy or difficult was it to use the service?").ExpectLabel("Very easy"); },
                    () => { Below("How easy or difficult was it to use the service?").ExpectLabel("Easy"); },
                    () => { Below("How easy or difficult was it to use the service?").ExpectLabel("Neither easy nor difficult"); },
                    () => { Below("How easy or difficult was it to use the service?").ExpectLabel("Difficult"); },
                    () => { Below("How easy or difficult was it to use the service?").ExpectLabel("Very difficult"); });
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

        [Test, Order(48)]
        public async Task Further_Feedback()
        {
            Expect("Further feedback");
            //ExpectXPath("//*[contains(.,'If you`re happy for us to get in touch with you about your feedback, please provide your details below.')]");

            Expect(What.Contains, "If you`re happy for us to get in touch with you about your feedback, please provide your details below.");
            Expect(What.Contains, "To find out how we'll handle your data, see our");

            ExpectLink("Privacy policy");

            ExpectLabel("Your email address (optional)");
            ExpectLabel("Your phone number (optional)");

            Expect(What.Contains, "If you're having difficulty using this service, please email ");
            ExpectLink("modernslaverystatements@homeoffice.gov.uk");
            Expect(What.Contains, "and we'll get back to you quickly.");
            await Task.CompletedTask.ConfigureAwait(false);
        }


    }
}