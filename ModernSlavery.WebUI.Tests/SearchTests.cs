using System.Linq;
using MockQueryable.Moq;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Tests.Common.Classes;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class SearchTests : AssertionHelper
    {

        private Mock<IDataRepository> mockDataRepo;

        #region Test Data

        private readonly OrganisationScope[] testScopeData = {
            new OrganisationScope {OrganisationScopeId = 123}, new OrganisationScope {OrganisationScopeId = 321}
        };

        #endregion

        [SetUp]
        public void BeforeEach()
        {
            mockDataRepo = MoqHelpers.CreateMockDataRepository();
        }

        [Test]
        [Description("")]
        public void ScopingViewModel_UnpackScope_FastTrack_IsNullOrWhiteSpace()
        {
            //ARRANGE
            mockDataRepo.Setup(r => r.GetAll<OrganisationScope>())
                .Returns(testScopeData.AsQueryable().BuildMock().Object);

            //ACT

            //ASSERT
        }

    }
}
