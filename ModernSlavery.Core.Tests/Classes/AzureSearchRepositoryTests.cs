using System;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Search;
using ModernSlavery.Tests.Common.Classes;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.Core.Tests.Classes
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class AzureSearchRepositoryTests
    {
        [SetUp]
        public void BeforeEach()
        {
        }

        [Test]
        public void AzureSearchRepository_Ctor_When_ServiceName_Is_Null_Then_Throw_ArgumentNullException()
        {
            // Arrange
            string nullTestServiceName = null;
            string nullTestIndexName = null;

            // Act
            TestDelegate testDelegate = () => new AzureEmployerSearchRepository(ConfigHelpers.SharedOptions,
                Mock.Of<IRecordLogger>(), nullTestServiceName, nullTestIndexName);

            // Assert
            Assert.That(
                testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With
                    .Message
                    .Contains("Value cannot be null." + Environment.NewLine + "Parameter name: serviceName"));
        }
    }
}