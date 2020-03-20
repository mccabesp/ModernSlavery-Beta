using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Database;
using ModernSlavery.Database.Classes;
using ModernSlavery.Extensions;
using ModernSlavery.Infrastructure.Data;
using NUnit.Framework;

namespace ModernSlavery.Tests.Common.Classes
{
    public static class TestHelpers
    {
        /// <summary>
        ///     Creates an InMemory SQLRespository and populates with entities
        /// </summary>
        /// <param name="dbObjects"></param>
        public static IDataRepository CreateInMemoryTestDatabase(params object[] dbObjects)
        {
            //Get the method name of the unit test or the parent
            var testName = TestContext.CurrentContext.Test.FullName;
            if (string.IsNullOrWhiteSpace(testName))
                testName = MethodBase.GetCurrentMethod().FindParentWithAttribute<TestAttribute>().Name;

            var optionsBuilder =
                new DbContextOptionsBuilder<DatabaseContext>().UseInMemoryDatabase(testName);

            optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            // show more detailed EF errors i.e. ReturnId value instead of '{ReturnId}' n the logs etc...
            optionsBuilder.EnableSensitiveDataLogging();

            var dbContext = new DatabaseContext(optionsBuilder.Options);
            if (dbObjects != null && dbObjects.Length > 0)
            {
                foreach (var item in dbObjects)
                {
                    var enumerable = item as IEnumerable<object>;
                    if (enumerable == null)
                        dbContext.Add(item);
                    else
                        dbContext.AddRange(enumerable);
                }

                dbContext.SaveChanges();
            }

            var dataRepo = new SqlRepository(dbContext);
            return dataRepo;
        }
    }
}