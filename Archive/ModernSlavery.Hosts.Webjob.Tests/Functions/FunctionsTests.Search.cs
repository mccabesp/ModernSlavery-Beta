﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models;
using ModernSlavery.Hosts.Webjob.Tests.TestHelpers;
using ModernSlavery.Tests.Common.Classes;
using ModernSlavery.Tests.Common.TestHelpers;
using Moq;
using NUnit.Framework;
using OrganisationHelper = ModernSlavery.Hosts.Webjob.Tests.TestHelpers.OrganisationHelper;

namespace ModernSlavery.Hosts.Webjob.Tests.Functions
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class UpdateSearchTests
    {
        [SetUp]
        public void BeforeEach()
        {
            //Create 10 test orgs with returns
            var orgs = OrganisationHelper.CreateTestOrganisations(10);

            //Instantiate the dependencies
            _functions = WebJobTestHelper.SetUp(orgs);
        }

        private Jobs.Functions _functions;

        [Test]
        [Ignore(
            "incomplete test, needs reviewing to confirm that indexes are NOT recreated as part of the running of this code")]
        public async Task Functions_UpdateSearch()
        {
            // Arrange
            var log = new Mock<ILogger>();
            var timespan = new TimeSpan();
            var schedule = new ConstantSchedule(timespan);
            var timer = new TimerInfo(schedule, new ScheduleStatus());

            // Act
            await _functions.UpdateSearchAsync(timer, log.Object);
        }

        [Test]
        [Description("Check indexes populated OK")]
        public async Task Functions_UpdateSearch_AddAllIndexes()
        {
            //ARRANGE
            //Ensure all orgs are in scope for current year
            IEnumerable<Organisation> orgs = _functions.SharedBusinessLogic.DataRepository.GetAll<Organisation>();

            //Add a random number of in scope orgs
            var inScope = Numeric.Rand(1, orgs.Count());
            OrganisationHelper.AddScopeStatus(ScopeStatuses.InScope, VirtualDateTime.Now.Year,
                orgs.Take(inScope).ToArray());

            //Add returns to remaining orgs 
            ReturnHelper.CreateTestReturns(orgs.Skip(inScope).ToArray(), VirtualDateTime.Now.Year);

            var log = new Mock<ILogger>();
            orgs = orgs
                .Where(
                    o => o.Status == OrganisationStatuses.Active
                         && (o.Returns.Any(r => r.Status == ReturnStatuses.Submitted)
                             || o.OrganisationScopes.Any(
                                 sc =>
                                     sc.Status == ScopeRowStatuses.Active
                                     && (sc.ScopeStatus == ScopeStatuses.InScope ||
                                         sc.ScopeStatus == ScopeStatuses.PresumedInScope))))
                .ToList();

            //ACT
            await _functions.UpdateSearchAsync(log.Object, "testadmin@user.com", true);

            //ASSERT

            //Check for correct number of indexes
            var documentCount = await _functions.SearchBusinessLogic.EmployerSearchRepository.GetDocumentCountAsync();
            Assert.That(documentCount == orgs.Count(), $"Expected '{documentCount}' indexes ");

            //Get the actual results
            var actualResults = await _functions.SearchBusinessLogic.EmployerSearchRepository.ListAsync();

            //Generate the expected results
            var expectedResults = orgs.Select(o => EmployerSearchModel.Create(o));

            //Check the results
            expectedResults.Compare(actualResults);
        }
    }
}