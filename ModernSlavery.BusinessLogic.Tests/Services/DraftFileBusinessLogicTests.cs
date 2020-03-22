using System;
using System.Threading;
using System.Threading.Tasks;
using ModernSlavery.BusinessLogic.Classes;
using ModernSlavery.BusinessLogic.Models.Submit;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.FileRepositories;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.BusinessLogic.Tests.Services
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class DraftFileBusinessLogicTests : BaseBusinessLogicTests
    {
        [SetUp]
        public void BeforeEach()
        {
            testFileRepository = new Mock<IFileRepository>();
            testOrganisationId = new Random().Next(100000, 150000);
            testSnapshotYear = VirtualDateTime.Now.AddYears(-1).Year;
            testUserId = new Random().Next(160000, 170000);

            SetupHelpers.SetupMockLogRecordGlobals();
        }

        private IDraftFileBusinessLogic testDraftFileBusinessLogic;
        private Mock<IFileRepository> testFileRepository;
        private long testOrganisationId;
        private int testSnapshotYear;
        private long testUserId;

        [Test]
        public async Task
            DraftFileBusinessLogic_CommitDraft_And_Then_RollbackDraft_Leaves_The_File_In_A_Consistent_StateAsync()
        {
            // Arrange
            var nicolasUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);
            var returnViewModelThatNicolasWillSendFirst = new ReturnViewModel {DiffMeanBonusPercent = 10.1m};
            var returnViewModelThatNicolasWillSendSecond =
                new ReturnViewModel {DiffMeanBonusPercent = 20.2m, DiffMedianBonusPercent = 22.2m};
            var returnViewModelThatNicolasWillSendThird = new ReturnViewModel
            {
                DiffMeanBonusPercent = 30.3m, DiffMedianBonusPercent = 33.3m,
                OrganisationSize = OrganisationSizes.Employees250To499
            };
            var returnViewModelThatNicolasWillSendFourth = new ReturnViewModel
            {
                DiffMeanBonusPercent = 40.4m, DiffMedianBonusPercent = 44.4m,
                OrganisationSize = OrganisationSizes.Employees1000To4999
            };

            // Act
            var emptyDraftLockedToNicolas =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    nicolasUserId);
            Assert.False(emptyDraftLockedToNicolas.HasDraftBeenModifiedDuringThisSession);

            var draftWithFirstLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatNicolasWillSendFirst,
                emptyDraftLockedToNicolas,
                nicolasUserId); // send data
            Assert.False(draftWithFirstLoadOfData.HasDraftBeenModifiedDuringThisSession,
                "this flag is set up by the front end");

            #region Confirm file status

            var intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    nicolasUserId);
            Assert.AreEqual(10.1m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.BackupDraftPath));

            #endregion

            var draftWithSecondLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatNicolasWillSendSecond,
                draftWithFirstLoadOfData,
                nicolasUserId); // send data
            Assert.False(
                draftWithSecondLoadOfData.HasDraftBeenModifiedDuringThisSession,
                "IsDraftDirty flag is set exclusively by the front end, so it always be 'false' unless the front end decides to change it.");

            #region Confirm file status

            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    nicolasUserId);
            Assert.AreEqual(20.2m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(22.2m, intermediateDraftInfo.ReturnViewModelContent.DiffMedianBonusPercent);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.BackupDraftPath));

            #endregion

            await testDraftFileBusinessLogic.CommitDraftAsync(draftWithSecondLoadOfData);
            Assert.False(await systemFileRepository.GetFileExistsAsync(draftWithSecondLoadOfData.BackupDraftPath));

            var draftWithThirdLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatNicolasWillSendThird,
                draftWithSecondLoadOfData,
                nicolasUserId); // send data
            Assert.False(
                draftWithThirdLoadOfData.HasDraftBeenModifiedDuringThisSession,
                "IsDraftDirty flag is set exclusively by the front end, so it always be 'false' unless the front end decides to change it.");

            #region Confirm file status

            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    nicolasUserId);
            Assert.AreEqual(30.3m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(33.3m, intermediateDraftInfo.ReturnViewModelContent.DiffMedianBonusPercent);
            Assert.AreEqual(OrganisationSizes.Employees250To499,
                intermediateDraftInfo.ReturnViewModelContent.OrganisationSize);

            #endregion

            var draftWithFourthLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatNicolasWillSendFourth,
                draftWithThirdLoadOfData,
                nicolasUserId); // send data
            Assert.False(
                draftWithFourthLoadOfData.HasDraftBeenModifiedDuringThisSession,
                "IsDraftDirty flag is set exclusively by the front end, so it always be 'false' unless the front end decides to change it.");

            #region Confirm file status

            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    nicolasUserId);
            Assert.AreEqual(40.4m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(44.4m, intermediateDraftInfo.ReturnViewModelContent.DiffMedianBonusPercent);
            Assert.AreEqual(OrganisationSizes.Employees1000To4999,
                intermediateDraftInfo.ReturnViewModelContent.OrganisationSize);

            #endregion

            await testDraftFileBusinessLogic.RollbackDraftAsync(draftWithFourthLoadOfData);

            #region Confirm file status

            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    nicolasUserId);
            Assert.AreEqual(20.2m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(22.2m, intermediateDraftInfo.ReturnViewModelContent.DiffMedianBonusPercent);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.BackupDraftPath));
            Assert.False(
                intermediateDraftInfo.HasDraftBeenModifiedDuringThisSession,
                "IsDraftDirty flag is set exclusively by the front end, so it always be 'false' unless the front end decides to change it.");

            #endregion

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(draftWithSecondLoadOfData);
        }

        [Test]
        public async Task
            DraftFileBusinessLogic_GetDraftIfAvailable_When_Json_Empty_And_Bak_File_Empty_Return_NullAsync()
        {
            // Arrange
            var robertUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);

            var emptyDraftLockedToRobert =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    robertUserId);

            // Act
            var availableDraft =
                await testDraftFileBusinessLogic.GetDraftIfAvailableAsync(testOrganisationId, testSnapshotYear);

            // Assert
            Assert.Null(availableDraft, "Both files exist but both empty, so no draft");

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToRobert);
        }

        [Test]
        public async Task DraftFileBusinessLogic_GetDraftIfAvailable_When_Json_Empty_And_Not_Bak_File_Return_NullAsync()
        {
            // Arrange
            var clareUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);

            var emptyDraftLockedToClare =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    clareUserId);
            await testDraftFileBusinessLogic.CommitDraftAsync(emptyDraftLockedToClare);

            // Act
            var availableDraft =
                await testDraftFileBusinessLogic.GetDraftIfAvailableAsync(testOrganisationId, testSnapshotYear);

            // Assert
            Assert.Null(
                availableDraft,
                "Json is empty (+ there isn't a bak file) as there isn't data to report, draft shouldn't be available");

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToClare);
        }

        [Test]
        public async Task
            DraftFileBusinessLogic_GetDraftIfAvailable_When_Json_Has_Data_And_Bak_File_Empty_Return_NullAsync()
        {
            // Arrange
            var oliviaUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);
            var returnViewModelThatOliviaWillSendFirst = new ReturnViewModel {DiffMedianBonusPercent = 11.1m};

            var emptyDraftLockedToOlivia =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    oliviaUserId);
            await testDraftFileBusinessLogic.UpdateAsync(returnViewModelThatOliviaWillSendFirst,
                emptyDraftLockedToOlivia, oliviaUserId);

            // Act
            var availableDraft =
                await testDraftFileBusinessLogic.GetDraftIfAvailableAsync(testOrganisationId, testSnapshotYear);

            // Assert
            Assert.Null(
                availableDraft,
                "Both files exist, as it is an inconsistent state (bak shouldn't be there if the process completed correctly), we used the backed up file as the reference because it's the most 'correct'. Bak file was empty, therefore null was expected");

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToOlivia);
        }

        [Test]
        public async Task
            DraftFileBusinessLogic_GetDraftIfAvailable_When_Json_Has_Data_And_Bak_File_Has_Data_Return_Bak_Draft_InfoAsync()
        {
            // Arrange
            var joeUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);
            var returnViewModelThatJoeWillSendFirst = new ReturnViewModel {DiffMedianBonusPercent = 11.1m};
            var returnViewModelThatJoeWillSendSecond =
                new ReturnViewModel {DiffMedianBonusPercent = 22.02m, DiffMeanHourlyPayPercent = 20.2m};

            var emptyDraftLockedToJoe =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear, joeUserId);
            var draftWithFirstLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatJoeWillSendFirst,
                emptyDraftLockedToJoe,
                joeUserId);
            await testDraftFileBusinessLogic.CommitDraftAsync(draftWithFirstLoadOfData);
            emptyDraftLockedToJoe =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear, joeUserId);
            draftWithFirstLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatJoeWillSendSecond,
                emptyDraftLockedToJoe,
                joeUserId);

            // Act
            var availableDraft =
                await testDraftFileBusinessLogic.GetDraftIfAvailableAsync(testOrganisationId, testSnapshotYear);

            // Assert
            Assert.NotNull(
                availableDraft,
                "Both files contain data, as it is an inconsistent state (bak shouldn't be there if the process completed correctly), we used the backed up data as it is the file we consider the most 'correct'");
            Assert.AreEqual(
                11.1m,
                availableDraft.ReturnViewModelContent.DiffMedianBonusPercent,
                "Information in the draft should be the one from the bak file");

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(availableDraft);
        }

        [Test]
        public async Task
            DraftFileBusinessLogic_GetDraftIfAvailable_When_Json_Has_Data_And_Not_Bak_File_Return_DraftAsync()
        {
            // Arrange
            var trevorUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);
            var returnViewModelThatTrevorWillSendFirst = new ReturnViewModel {DiffMedianBonusPercent = 11.1m};

            var emptyDraftLockedToTrevor =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    trevorUserId);
            var draftWithFirstLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatTrevorWillSendFirst,
                emptyDraftLockedToTrevor,
                trevorUserId);
            await testDraftFileBusinessLogic.CommitDraftAsync(draftWithFirstLoadOfData);

            // Act
            var availableDraft =
                await testDraftFileBusinessLogic.GetDraftIfAvailableAsync(testOrganisationId, testSnapshotYear);

            // Assert
            Assert.NotNull(availableDraft,
                "Json has data and there isn't a bak file, this is a consistent state, so a draft is available");

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(availableDraft);
        }

        [Test]
        public async Task DraftFileBusinessLogic_GetDraftIfAvailable_When_Not_Json_Return_NullAsync()
        {
            // Arrange
            testDraftFileBusinessLogic =
                new DraftFileBusinessLogic(null, new SystemFileRepository(new StorageOptions()));

            // Act
            var availableDraft =
                await testDraftFileBusinessLogic.GetDraftIfAvailableAsync(testOrganisationId, testSnapshotYear);

            // Assert
            Assert.Null(availableDraft, "Json isn't available, so it is expected that 'available draft' is null");
        }

        [Test]
        public async Task DraftFileBusinessLogic_GetExistingOrNew_Creates_Empty_Json_And_Bak_FilesAsync()
        {
            // Arrange
            var jackUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);

            // Act
            var emptyDraftLockedToJack =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    jackUserId);
            Assert.True(await systemFileRepository.GetFileExistsAsync(emptyDraftLockedToJack.DraftPath),
                "Expected a draft json file");
            Assert.True(await systemFileRepository.GetFileExistsAsync(emptyDraftLockedToJack.BackupDraftPath),
                "Expected a backup file");

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToJack);
        }

        [Test]
        public async Task DraftFileBusinessLogic_GetExistingOrNew_User_Can_Create_And_Update_DraftAsync()
        {
            // Arrange
            var lizzyUserId = testUserId;
            var fileRepo = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic =
                new DraftFileBusinessLogic(null, new SystemFileRepository(new StorageOptions()));
            var expectedDraft = new Draft(testOrganisationId, testSnapshotYear, true, VirtualDateTime.Now, lizzyUserId,
                fileRepo.RootDir);
            var returnViewModelChangedByLizzy = new ReturnViewModel {DiffMeanBonusPercent = 78.2m};

            // Act
            var draftLockedToLizzy =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    lizzyUserId);
            var updatedDraft = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelChangedByLizzy,
                draftLockedToLizzy,
                lizzyUserId);

            // Assert
            Assert.Multiple(
                () =>
                {
                    Assert.NotNull(updatedDraft.ReturnViewModelContent);
                    Assert.AreEqual(expectedDraft.DraftPath, updatedDraft.DraftPath);
                    Assert.AreEqual(expectedDraft.DraftFilename, updatedDraft.DraftFilename);
                    Assert.AreEqual(expectedDraft.IsUserAllowedAccess, updatedDraft.IsUserAllowedAccess);
                    Assert.AreEqual(expectedDraft.LastWrittenByUserId, updatedDraft.LastWrittenByUserId);
                    Assert.AreEqual(expectedDraft.BackupDraftFilename, updatedDraft.BackupDraftFilename);
                    Assert.AreEqual(expectedDraft.BackupDraftPath, updatedDraft.BackupDraftPath);
                    Assert.False(
                        updatedDraft.HasDraftBeenModifiedDuringThisSession,
                        "IsDraftDirty flag is set exclusively by the front end, so it always be 'false' unless the front end decides to change it.");
                });

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(updatedDraft);
        }

        [Test]
        public async Task DraftFileBusinessLogic_GetExistingOrNew_User_Wont_Be_Allowed_To_Access_A_Locked_FileAsync()
        {
            // Arrange
            var queenElisabethId = testUserId;
            var philipDukeOfEdinburghId = testUserId * 25;
            testDraftFileBusinessLogic =
                new DraftFileBusinessLogic(null, new SystemFileRepository(new StorageOptions()));

            // Act
            var draftLockedToQueenElisabeth = await testDraftFileBusinessLogic.GetExistingOrNewAsync(
                testOrganisationId,
                testSnapshotYear,
                queenElisabethId);
            var draftRequestedByPhilip = await testDraftFileBusinessLogic.GetExistingOrNewAsync(
                testOrganisationId,
                testSnapshotYear,
                philipDukeOfEdinburghId);

            // Assert
            Assert.False(
                draftRequestedByPhilip.IsUserAllowedAccess,
                "Philip shouldn't be allowed access the draft file as it's in use by Queen Elisabeth");
            Assert.AreEqual(
                queenElisabethId,
                draftRequestedByPhilip.LastWrittenByUserId,
                $"The last person writing onto the draft file is expected to have been the Queen userId '{queenElisabethId}'");

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(draftLockedToQueenElisabeth);
        }

        [Test]
        public async Task
            DraftFileBusinessLogic_GetExistingOrNew_When_File_Does_Not_Exist_Creates_It_And_Locks_ItAsync()
        {
            // Arrange
            var fileRepo = new SystemFileRepository(new StorageOptions());

            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepo);
            var expectedDraft = new Draft(testOrganisationId, testSnapshotYear, true, VirtualDateTime.Now, testUserId,
                fileRepo.RootDir);

            // Act
            var actualDraft =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    testUserId);

            // Assert
            Assert.Multiple(
                () =>
                {
                    Assert.AreEqual(expectedDraft.ReturnViewModelContent, actualDraft.ReturnViewModelContent);
                    Assert.AreEqual(expectedDraft.DraftPath, actualDraft.DraftPath);
                    Assert.AreEqual(expectedDraft.DraftFilename, actualDraft.DraftFilename);
                    Assert.AreEqual(expectedDraft.IsUserAllowedAccess, actualDraft.IsUserAllowedAccess);
                    Assert.AreEqual(expectedDraft.LastWrittenByUserId, actualDraft.LastWrittenByUserId);
                    Assert.AreEqual(expectedDraft.BackupDraftFilename, actualDraft.BackupDraftFilename);
                    Assert.AreEqual(expectedDraft.BackupDraftPath, actualDraft.BackupDraftPath);
                    Assert.AreEqual(expectedDraft.HasDraftBeenModifiedDuringThisSession,
                        actualDraft.HasDraftBeenModifiedDuringThisSession);
                    // Assert.AreEqual(expected.LastWrittenDateTime, actual.LastWrittenDateTime); // todo: consider 'equal' when 45 sec apart
                });

            // Remove test file
            await testDraftFileBusinessLogic.DiscardDraftAsync(actualDraft);
        }

        [Test]
        public async Task DraftFileBusinessLogic_GetExistingOrNew_When_File_Exists_And_Locks_It_To_Calling_UserAsync()
        {
            // Arrange
            var fileRepo = new SystemFileRepository(new StorageOptions());

            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepo);
            var expectedDraft = new Draft(testOrganisationId, testSnapshotYear, true, VirtualDateTime.Now, testUserId,
                fileRepo.RootDir);

            // Act
            await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear, testUserId);
            var actualDraft =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    testUserId);

            // Assert
            Assert.Multiple(
                () =>
                {
                    Assert.AreEqual(expectedDraft.ReturnViewModelContent, actualDraft.ReturnViewModelContent);
                    Assert.AreEqual(expectedDraft.DraftPath, actualDraft.DraftPath);
                    Assert.AreEqual(expectedDraft.DraftFilename, actualDraft.DraftFilename);
                    Assert.AreEqual(expectedDraft.IsUserAllowedAccess, actualDraft.IsUserAllowedAccess);
                    Assert.AreEqual(expectedDraft.LastWrittenByUserId, actualDraft.LastWrittenByUserId);
                    Assert.AreEqual(expectedDraft.BackupDraftFilename, actualDraft.BackupDraftFilename);
                    Assert.AreEqual(expectedDraft.BackupDraftPath, actualDraft.BackupDraftPath);
                    Assert.AreEqual(expectedDraft.HasDraftBeenModifiedDuringThisSession,
                        actualDraft.HasDraftBeenModifiedDuringThisSession);
                });

            // Remove test file
            await testDraftFileBusinessLogic.DiscardDraftAsync(actualDraft);
        }

        [Test]
        public async Task DraftFileBusinessLogic_GetExistingOrNew_When_File_Locked_It_Reports_Not_Allowed_AccessAsync()
        {
            // Arrange
            var fileRepo = new SystemFileRepository(new StorageOptions());

            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepo);
            var userIdLockingTheDraft = testUserId * 45;
            var userIdRequestingTheDraft = testUserId;
            var expectedDraft = new Draft(testOrganisationId, testSnapshotYear, true, VirtualDateTime.Now,
                userIdLockingTheDraft, fileRepo.RootDir);

            // Act
            await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                userIdLockingTheDraft);
            var actualDraft = await testDraftFileBusinessLogic.GetExistingOrNewAsync(
                testOrganisationId,
                testSnapshotYear,
                userIdRequestingTheDraft);

            // Assert
            Assert.Multiple(
                () =>
                {
                    Assert.AreEqual(expectedDraft.ReturnViewModelContent, actualDraft.ReturnViewModelContent);
                    Assert.AreEqual(expectedDraft.DraftPath, actualDraft.DraftPath);
                    Assert.AreEqual(expectedDraft.DraftFilename, actualDraft.DraftFilename);
                    Assert.False(
                        actualDraft.IsUserAllowedAccess,
                        $"The file is locked by user '{userIdLockingTheDraft}', so the access for a different user '{userIdRequestingTheDraft}' must be returned as 'false'");
                    Assert.AreEqual(
                        expectedDraft.LastWrittenByUserId,
                        actualDraft.LastWrittenByUserId,
                        $"The user that last wrote on the file it is expected to have been {userIdLockingTheDraft}");
                    Assert.AreEqual(expectedDraft.BackupDraftFilename, actualDraft.BackupDraftFilename);
                    Assert.AreEqual(expectedDraft.BackupDraftPath, actualDraft.BackupDraftPath);
                    Assert.AreEqual(expectedDraft.HasDraftBeenModifiedDuringThisSession,
                        actualDraft.HasDraftBeenModifiedDuringThisSession);
                });

            // Remove test file
            await testDraftFileBusinessLogic.DiscardDraftAsync(expectedDraft);
        }

        [Test]
        public async Task DraftFileBusinessLogic_KeepDraftFileLockedToUser_When_Not_Json_Return_NullAsync()
        {
            // Arrange
            var stewardUserId = testUserId;
            var fileRepo = new SystemFileRepository(new StorageOptions());

            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepo);
            var emptyDraftLockedToSteward =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    stewardUserId);
            var creationTimeStamp = emptyDraftLockedToSteward.LastWrittenDateTime;
            emptyDraftLockedToSteward.HasDraftBeenModifiedDuringThisSession = true;
            Thread.Sleep(
                1000); // Delay between creating the file and the subsequent request to keep it locked to steward.

            // Act

            // the object 'emptyDraftLockedToSteward' will be modified by the method
            await testDraftFileBusinessLogic.KeepDraftFileLockedToUserAsync(emptyDraftLockedToSteward, stewardUserId);

            // Assert
            Assert.NotNull(emptyDraftLockedToSteward,
                "Expected a draft to be available as we're only requesting a lock");
            Assert.Null(emptyDraftLockedToSteward.ReturnViewModelContent,
                "Haven't added any data, so the content should be empty");
            Assert.True(emptyDraftLockedToSteward.IsUserAllowedAccess, "Steward must be able to access the draft");
            Assert.AreEqual(stewardUserId, emptyDraftLockedToSteward.LastWrittenByUserId,
                "Should have been locked to Steward");
            Assert.True(
                emptyDraftLockedToSteward.HasDraftBeenModifiedDuringThisSession,
                "the method 'KeepDraftFileLockedToUser' is expected to modified some fields, but 'IsDraftDirty' must be left alone, as it is the front end's responsibility to determine if the draft contains new values or not - only the individual pages can determine if the user has typed something on screen");
            Assert.True(
                creationTimeStamp < emptyDraftLockedToSteward.LastWrittenDateTime,
                $"The draft file's metadata has been changed to reflect that Steward wants to keep the file locked to him, so it must have a timeStamp '{emptyDraftLockedToSteward.LastWrittenDateTime}' different-and-older than the initial file '{creationTimeStamp}'");

            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToSteward);
        }

        [Test]
        public async Task DraftFileBusinessLogic_RestartDraft_Both_Files_Json_Deleted_Bak_Renamed_As_JsonAsync()
        {
            // Arrange
            var aliciaUserId = testUserId;
            var fileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepository);
            var emptyDraftLockedToAlicia =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    aliciaUserId);

            // Confirm both files are there before we start
            Assert.True(await fileRepository.GetFileExistsAsync(emptyDraftLockedToAlicia.DraftPath));
            Assert.True(await fileRepository.GetFileExistsAsync(emptyDraftLockedToAlicia.BackupDraftPath));

            // Act
            await testDraftFileBusinessLogic.RestartDraftAsync(testOrganisationId, testSnapshotYear, aliciaUserId);

            // Assert
            // Bak was rolled back and both files are recreated
            Assert.True(await fileRepository.GetFileExistsAsync(emptyDraftLockedToAlicia.DraftPath));
            Assert.True(await fileRepository.GetFileExistsAsync(emptyDraftLockedToAlicia.BackupDraftPath));

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToAlicia);
        }

        [Test]
        public async Task DraftFileBusinessLogic_RestartDraft_No_Files_Does_Not_FailAsync()
        {
            // Arrange
            var simonUserId = testUserId;
            var fileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepository);
            var tempDraft = new Draft(testOrganisationId, testSnapshotYear, fileRepository.RootDir);

            // Act
            await testDraftFileBusinessLogic.RestartDraftAsync(testOrganisationId, testSnapshotYear, simonUserId);

            // Assert
            Assert.False(await fileRepository.GetFileExistsAsync(tempDraft.BackupDraftPath));
        }

        [Test]
        public async Task DraftFileBusinessLogic_RestartDraft_Only_Bak_Renames_As_JsonAsync()
        {
            // Arrange
            var andreaUserId = testUserId;
            var fileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepository);
            var emptyDraftLockedToAndrea =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    andreaUserId);
            await fileRepository.DeleteFileAsync(emptyDraftLockedToAndrea.DraftPath);

            // Act
            await testDraftFileBusinessLogic.RestartDraftAsync(testOrganisationId, testSnapshotYear, andreaUserId);

            // Asserts 
            // Bak was rolled back and both files are recreated
            Assert.True(await fileRepository.GetFileExistsAsync(emptyDraftLockedToAndrea.DraftPath));
            Assert.True(await fileRepository.GetFileExistsAsync(emptyDraftLockedToAndrea.BackupDraftPath));

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToAndrea);
        }

        [Test]
        public async Task DraftFileBusinessLogic_RestartDraft_Only_Json_Ignored_Nothing_To_Rollback_FromAsync()
        {
            // Arrange
            var anneUserId = testUserId;
            var fileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, fileRepository);
            var emptyDraftLockedToAnne =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    anneUserId);
            await testDraftFileBusinessLogic.CommitDraftAsync(emptyDraftLockedToAnne);

            // Act
            await testDraftFileBusinessLogic.RestartDraftAsync(testOrganisationId, testSnapshotYear, anneUserId);

            // Assert
            Assert.False(await fileRepository.GetFileExistsAsync(emptyDraftLockedToAnne.BackupDraftPath));

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(emptyDraftLockedToAnne);
        }

        [Test]
        public async Task DraftFileBusinessLogic_RollbackDraft_Maintains_Initial_Information_Added_In_One_SessionAsync()
        {
            // Arrange
            var kathyUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);
            var returnViewModelThatKathyWillSendFirst = new ReturnViewModel {DiffMeanBonusPercent = 20.2m};
            var returnViewModelThatKathyWillSendSecond = new ReturnViewModel
                {DiffMeanBonusPercent = 30.3m, DiffMedianBonusPercent = 33.3m};
            var returnViewModelThatKathyWillSendThird = new ReturnViewModel
            {
                DiffMeanBonusPercent = 40.4m, DiffMedianBonusPercent = 44.4m,
                OrganisationSize = OrganisationSizes.Employees250To499
            };

            // Act
            var emptyDraftLockedToKathy =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    kathyUserId);
            Assert.True(await systemFileRepository.GetFileExistsAsync(emptyDraftLockedToKathy.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(emptyDraftLockedToKathy.BackupDraftPath));

            var draftWithFirstLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatKathyWillSendFirst,
                emptyDraftLockedToKathy,
                kathyUserId); // send data
            var intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    kathyUserId);
            Assert.AreEqual(20.2m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.BackupDraftPath));

            var draftWithSecondLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatKathyWillSendSecond,
                draftWithFirstLoadOfData,
                kathyUserId); // send data
            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    kathyUserId);
            Assert.AreEqual(30.3m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(33.3m, intermediateDraftInfo.ReturnViewModelContent.DiffMedianBonusPercent);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithSecondLoadOfData.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithSecondLoadOfData.BackupDraftPath));

            var draftWithThirdLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatKathyWillSendThird,
                draftWithSecondLoadOfData,
                kathyUserId); // send data
            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    kathyUserId);
            Assert.AreEqual(40.4m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(44.4m, intermediateDraftInfo.ReturnViewModelContent.DiffMedianBonusPercent);
            Assert.AreEqual(OrganisationSizes.Employees250To499,
                intermediateDraftInfo.ReturnViewModelContent.OrganisationSize);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithThirdLoadOfData.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithThirdLoadOfData.BackupDraftPath));

            await testDraftFileBusinessLogic.RollbackDraftAsync(draftWithThirdLoadOfData);

            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    kathyUserId);
            Assert.Null(intermediateDraftInfo.ReturnViewModelContent);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.BackupDraftPath));

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(draftWithSecondLoadOfData);
        }

        [Test]
        public async Task
            DraftFileBusinessLogic_Update_When_Data_Is_Sent_Repeatedly_Draft_Integrity_Is_MaintainedAsync()
        {
            // Arrange
            var maryUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);
            var returnViewModelThatMaryWillSendTwice = new ReturnViewModel {DiffMeanBonusPercent = 65.3m};

            // Act
            var emptyDraftLockedToMary =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    maryUserId);
            var intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    maryUserId);
            Assert.Null(intermediateDraftInfo.ReturnViewModelContent, "Expected file to be empty");
            Assert.AreEqual(maryUserId, intermediateDraftInfo.LastWrittenByUserId, "Should have been locked to Mary");
            Assert.True(await systemFileRepository.GetFileExistsAsync(intermediateDraftInfo.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(intermediateDraftInfo.BackupDraftPath));

            var draftWithData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatMaryWillSendTwice,
                emptyDraftLockedToMary,
                maryUserId);
            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    maryUserId);
            Assert.AreEqual(65.3m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(maryUserId, intermediateDraftInfo.LastWrittenByUserId, "Should have been locked to Mary");
            Assert.True(await systemFileRepository.GetFileExistsAsync(intermediateDraftInfo.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(intermediateDraftInfo.BackupDraftPath));

            var updatedDraft = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatMaryWillSendTwice,
                draftWithData,
                maryUserId);
            intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    maryUserId);
            Assert.AreEqual(65.3m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.AreEqual(maryUserId, intermediateDraftInfo.LastWrittenByUserId, "Should have been locked to Mary");
            Assert.True(await systemFileRepository.GetFileExistsAsync(intermediateDraftInfo.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(intermediateDraftInfo.BackupDraftPath));

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(updatedDraft);
        }

        [Test]
        public async Task DraftFileBusinessLogic_Update_When_Json_Exists_But_Does_Not_Have_Data_Backup_Is_CreatedAsync()
        {
            // Arrange
            var fredUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);

            // Act
            var emptyDraftLockedToFred =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    fredUserId);
            var updatedDraft =
                await testDraftFileBusinessLogic.UpdateAsync(new ReturnViewModel(), emptyDraftLockedToFred, fredUserId);

            // Assert
            Assert.True(await systemFileRepository.GetFileExistsAsync(updatedDraft.BackupDraftPath));

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(updatedDraft);
        }

        [Test]
        public async Task DraftFileBusinessLogic_Update_When_New_Data_Is_Received_Backup_Is_MaintainedAsync()
        {
            // Arrange
            var dominicUserId = testUserId;
            var systemFileRepository = new SystemFileRepository(new StorageOptions());
            testDraftFileBusinessLogic = new DraftFileBusinessLogic(null, systemFileRepository);
            var returnViewModelThatDominicWillSendFirst = new ReturnViewModel {DiffMeanBonusPercent = 40.4m};
            var returnViewModelThatDominicWillSendSecond =
                new ReturnViewModel {DiffMeanBonusPercent = 50.5m, DiffMedianBonusPercent = 55.5m};

            // Act
            var emptyDraftLockedToDominic =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    dominicUserId);
            Assert.True(await systemFileRepository.GetFileExistsAsync(emptyDraftLockedToDominic.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(emptyDraftLockedToDominic.BackupDraftPath));

            var draftWithFirstLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatDominicWillSendFirst,
                emptyDraftLockedToDominic,
                dominicUserId); // send data
            var intermediateDraftInfo =
                await testDraftFileBusinessLogic.GetExistingOrNewAsync(testOrganisationId, testSnapshotYear,
                    dominicUserId);
            Assert.AreEqual(40.4m, intermediateDraftInfo.ReturnViewModelContent.DiffMeanBonusPercent);
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.DraftPath));
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithFirstLoadOfData.BackupDraftPath));

            var draftWithSecondLoadOfData = await testDraftFileBusinessLogic.UpdateAsync(
                returnViewModelThatDominicWillSendSecond,
                draftWithFirstLoadOfData,
                dominicUserId); // send data
            Assert.True(await systemFileRepository.GetFileExistsAsync(draftWithSecondLoadOfData.BackupDraftPath));

            // Cleanup
            await testDraftFileBusinessLogic.DiscardDraftAsync(draftWithSecondLoadOfData);
        }
    }
}