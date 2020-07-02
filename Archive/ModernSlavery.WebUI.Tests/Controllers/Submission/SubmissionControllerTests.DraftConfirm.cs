using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ModernSlavery.BusinessDomain.Submission;
using ModernSlavery.BusinessDomain;
using ModernSlavery.BusinessDomain.Shared.Interfaces;
using ModernSlavery.BusinessDomain.Shared.Models;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.Infrastructure.Storage;
using ModernSlavery.Infrastructure.Storage.FileRepositories;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using ModernSlavery.WebUI.Submission.Classes;
using ModernSlavery.WebUI.Submission.Controllers;
using ModernSlavery.WebUI.Tests.TestHelpers;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.WebUI.Tests.Controllers.Submission
{

    public partial class SubmitControllerTests
    {

        [Test]
        [Description("DraftComplete should fail when male bonus is zero but provided mean or median bonus difference")]
        public async Task SubmitController_DraftComplete_POST_MaleBonusIsZero_ShowsErrors()
        {
            // Arrange
            var user = new User {UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            var organisation = new Core.Entities.Organisation {OrganisationId = 1, SectorType = SectorTypes.Public};
            var userOrganisation = new UserOrganisation {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            DateTime PrivateAccountingDate = SectorTypeHelper.SnapshotDateHelper.PrivateAccountingDate;

            var returnViewModel = new ReturnViewModel {
                AccountingDate = PrivateAccountingDate,
                MaleMedianBonusPayPercent = 0,
                FemaleMedianBonusPayPercent = 50,
                DiffMeanBonusPercent = -50,
                DiffMedianBonusPercent = -50,
                DiffMedianHourlyPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private
            };

            var controller = UiTestHelper.GetController<SubmissionController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(returnViewModel);

            var sharedBusinessLogic = UiTestHelper.DIContainer.Resolve<ISharedBusinessLogic>();
            var testDraftFileBL = new DraftFileBusinessLogic(null,new SystemFileRepository(new StorageOptions()));
            var testSubmissionService = new SubmissionService(sharedBusinessLogic, Mock.Of<ISubmissionBusinessLogic>(), Mock.Of<IScopeBusinessLogic>(), testDraftFileBL);
            var testPresenter = new SubmissionPresenter(testSubmissionService, ConfigHelpers.SubmissionOptions, null);


            returnViewModel.ReportInfo.Draft = await testPresenter.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            // Act
            controller.StashModel(returnViewModel);
            controller.ReportingOrganisationId = 1;
            var result = await controller.SaveDraftAsync(returnViewModel) as ViewResult;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.Null(result.ViewName, "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.AreEqual(
                "Do not enter a bonus difference if 0% of your male employees received a bonus",
                result.ViewData.ModelState["DiffMedianBonusPercent"].Errors[0].ErrorMessage);
            Assert.AreEqual(
                "Do not enter a bonus difference if 0% of your male employees received a bonus",
                result.ViewData.ModelState["DiffMeanBonusPercent"].Errors[0].ErrorMessage);
        }

    }

}
