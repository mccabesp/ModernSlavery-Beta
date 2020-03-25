using Microsoft.AspNetCore.Http;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.WebUI.Shared.Interfaces;
using ModernSlavery.WebUI.Viewing.Classes;
using ModernSlavery.WebUI.Viewing.Presenters;
using Moq;
using NUnit.Framework;
using HttpContext = Microsoft.AspNetCore.Http.HttpContext;

namespace ModernSlavery.WebUI.Tests.Services.Compare
{

    public class LoadsAndSavesCookieTests
    {

        private readonly Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();

        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        [SetUp]
        public void BeforeEach()
        {
            mockHttpContext.Setup(x => x.Request.Cookies.ContainsKey(It.Is<string>(arg => arg == CookieNames.LastCompareQuery)))
                .Returns(true);

            mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(mockHttpContext.Object);
        }

        [TestCase]
        [TestCase("AAAAAAAA", "BBBBBBBB", "CCCCCCCC", "DDDDDDDD", "FFFFFFFF")]
        public void LoadsEmployersFromCookie(params string[] expectedEmployerIds)
        {
            // Arrange
            mockHttpContext.Setup(x => x.Request.Cookies[It.Is<string>(arg => arg == CookieNames.LastCompareQuery)])
                .Returns(string.Join(",", expectedEmployerIds));

            var testPresenter = new ComparePresenter(
                MoqHelpers.CreateIOptionsSnapshotMock(new ViewingOptions()),
                mockHttpContextAccessor.Object,
                Mock.Of<IHttpSession>());

            // Act
            testPresenter.LoadComparedEmployersFromCookie();

            // Assert
            Assert.AreEqual(
                expectedEmployerIds.Length,
                testPresenter.BasketItemCount,
                $"Expected basket to contain {expectedEmployerIds.Length} employers");
            Assert.IsTrue(testPresenter.ComparedEmployers.Value.Contains(expectedEmployerIds), "Expected employer ids to match basket items");
        }

        [TestCase]
        public void ClearsBasketBeforeLoadingFromCookie()
        {
            // Arrange
            mockHttpContext.Setup(x => x.Request.Cookies[It.Is<string>(arg => arg == CookieNames.LastCompareQuery)])
                .Returns("12345678");

            var testPresenter = new ComparePresenter(
                MoqHelpers.CreateIOptionsSnapshotMock(new ViewingOptions()),
                mockHttpContextAccessor.Object,
                Mock.Of<IHttpSession>());

            var testPreviousIds = new[] {"AAA", "BBB", "CCC"};
            testPresenter.AddRangeToBasket(testPreviousIds);

            // Act
            testPresenter.LoadComparedEmployersFromCookie();

            // Assert
            Assert.AreEqual(1, testPresenter.BasketItemCount, "Expected basket to contain 1 employer");
            Assert.IsFalse(
                testPresenter.ComparedEmployers.Value.Contains(testPreviousIds),
                "Expected previous employer ids to be cleared from basket");
        }

        [TestCase]
        [Ignore("Need to create a mock HttpRequest object to pass to SaveComparedEmployersToCookie, but I don't have time to do this now'")]
        public void SavesComparedEmployersToCookie()
        {
            // Arrange
            var AppendWasCalled = false;
            mockHttpContext.Setup(x => x.Request.Cookies[It.Is<string>(arg => arg == CookieNames.LastCompareQuery)])
                .Returns("12345678");

            var testPresenter = new ComparePresenter(
                MoqHelpers.CreateIOptionsSnapshotMock(new ViewingOptions()),
                mockHttpContextAccessor.Object,
                Mock.Of<IHttpSession>());

            var testIds = new[] {"AAA", "BBB", "CCC"};
            testPresenter.AddRangeToBasket(testIds);

            mockHttpContext.Setup(
                    x => x.Response.Cookies.Append(
                        It.Is<string>(arg => arg == CookieNames.LastCompareQuery),
                        It.IsAny<string>(),
                        It.IsAny<CookieOptions>()))
                .Callback(
                    (string key, string value, CookieOptions options) => {
                        // Assert
                        Assert.AreEqual(string.Join(",", testIds), value);
                        AppendWasCalled = true;
                    });

            // Act
            testPresenter.SaveComparedEmployersToCookie(null);

            // Assert
            Assert.IsTrue(AppendWasCalled);
        }

    }

}
