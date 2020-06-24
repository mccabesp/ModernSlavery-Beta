﻿using ModernSlavery.BusinessLogic.Submission.Models;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Classes
{
    [TestFixture]
    public class DraftTests
    {
        [Test]
        public void HasContent_When_ReturnViewModelContent_Is_Not_Null_Returns_True()
        {
            // Arrange
            var testDraft = new Draft(default, default, default) {ReturnViewModelContent = new ReturnViewModel()};

            // Act
            var actualHasContent = testDraft.HasContent();

            // Assert
            Assert.True(actualHasContent,
                "If there is a ReturnViewModelContent inside the Draft, the method is expected to return true");
        }

        [Test]
        public void HasContent_When_ReturnViewModelContent_Is_Null_Returns_False()
        {
            // Arrange
            var testDraft = new Draft(default, default, default) {ReturnViewModelContent = null};

            // Act
            var actualHasContent = testDraft.HasContent();

            // Assert
            Assert.False(actualHasContent,
                "If there isn't a ReturnViewModelContent the method is expected to return false");
        }
    }
}