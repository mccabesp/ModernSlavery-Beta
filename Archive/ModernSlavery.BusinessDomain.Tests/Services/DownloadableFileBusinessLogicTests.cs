﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Admin;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Services
{
    [TestFixture]
    public class DownloadableFileBusinessLogicTests
    {
        [Test]
        public async Task GetListOfDownloadableItemsFromPathAsync_Returns_At_Least_The_Parent_Folder()
        {
            // Arrange
            var processedLogsPath = "Some/Random/Path";
            var configurableFileRepository = new Mock<IFileRepository>();
            configurableFileRepository
                .Setup(x => x.GetDirectoriesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Array.Empty<string>());

            configurableFileRepository
                .Setup(x => x.GetFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Array.Empty<string>());

            var downloadableFileBusinessLogic = new DownloadableFileBusinessLogic(configurableFileRepository.Object);

            // Act
            var actualListOfDownloadableItems =
                await downloadableFileBusinessLogic.GetListOfDownloadableItemsFromPathAsync(processedLogsPath);

            // Assert
            Assert.NotNull(actualListOfDownloadableItems);
            var parentDownloadableItem = actualListOfDownloadableItems.FirstOrDefault(x => x.Name == "..");
            Assert.NotNull(parentDownloadableItem,
                "Expected at least one parent folder with special name two dots '..' ");
            Assert.AreEqual("Some/Random", parentDownloadableItem.Filepath);
        }

        [Test]
        public async Task GetListOfDownloadableItemsFromPathAsync_Returns_Folders_And_Files()
        {
            // Arrange
            var processedLogsPath = "Some/Random/Path";
            var configurableFileRepository = new Mock<IFileRepository>();
            configurableFileRepository
                .Setup(x => x.GetDirectoriesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] {"Root/Directory_1"});

            configurableFileRepository
                .Setup(x => x.GetFilesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(new[] {"path/File1.csv"});

            var downloadableFileBusinessLogic = new DownloadableFileBusinessLogic(configurableFileRepository.Object);

            // Act
            var actualListOfDownloadableItems =
                await downloadableFileBusinessLogic.GetListOfDownloadableItemsFromPathAsync(processedLogsPath);

            // Assert
            Assert.NotNull(actualListOfDownloadableItems);
            var directoryDownloadableItem = actualListOfDownloadableItems.FirstOrDefault(x => x.Name == "Directory_1");
            Assert.NotNull(directoryDownloadableItem, "Expected one folder with name 'Directory_1' ");
            Assert.AreEqual("Root/Directory_1", ((DownloadableDirectory) directoryDownloadableItem).Filepath);


            var fileDownloadableItem = actualListOfDownloadableItems.FirstOrDefault(x => x.Name == "File1.csv");
            Assert.NotNull(fileDownloadableItem, "Expected one file with name 'File1.csv' ");
            Assert.AreEqual("path/File1.csv", ((DownloadableFile) fileDownloadableItem).Filepath);
        }
    }
}