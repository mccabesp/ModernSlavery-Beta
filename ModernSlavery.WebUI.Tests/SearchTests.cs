using System.Linq;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Entities;
using ModernSlavery.Tests.Common.Classes;
using MockQueryable.Moq;
using Moq;

using NUnit.Framework;

namespace ModernSlavery.Tests
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
