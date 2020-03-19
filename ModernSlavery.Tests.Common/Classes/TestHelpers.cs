using System.Collections.Generic;
using System.Reflection;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using ModernSlavery.Database;
using ModernSlavery.Infrastructure.Data;

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
            string testName = TestContext.CurrentContext.Test.FullName;
            if (string.IsNullOrWhiteSpace(testName))
            {
                testName = MethodBase.GetCurrentMethod().FindParentWithAttribute<TestAttribute>().Name;
            }

            DbContextOptionsBuilder<DatabaseContext> optionsBuilder =
                new DbContextOptionsBuilder<DatabaseContext>().UseInMemoryDatabase(testName);

            optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            // show more detailed EF errors i.e. ReturnId value instead of '{ReturnId}' n the logs etc...
            optionsBuilder.EnableSensitiveDataLogging();

            var dbContext = new DatabaseContext(optionsBuilder.Options);
            if (dbObjects != null && dbObjects.Length > 0)
            {
                foreach (object item in dbObjects)
                {
                    var enumerable = item as IEnumerable<object>;
                    if (enumerable == null)
                    {
                        dbContext.Add(item);
                    }
                    else
                    {
                        dbContext.AddRange(enumerable);
                    }
                }

                dbContext.SaveChanges();
            }

            var dataRepo = new SqlRepository(dbContext);
            return dataRepo;
        }

    }
}
