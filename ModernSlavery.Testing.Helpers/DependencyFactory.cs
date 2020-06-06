using Microsoft.EntityFrameworkCore;
using ModernSlavery.Infrastructure.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModernSlavery.Core.Extensions;
using NUnit.Framework;

namespace ModernSlavery.Testing.Helpers
{
    public static class DependencyFactory
    {
        public static DatabaseContext CreateInMemoryDatabaseContext(string databaseName=null)
        {
            //Get the method name of the unit test or the parent
            if (string.IsNullOrWhiteSpace(databaseName)) databaseName=TestContextHelper.GetTestContextName(Guid.NewGuid().ToShortString());
            
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>().UseInMemoryDatabase(databaseName);

            // show more detailed EF errors i.e. ReturnId value instead of '{ReturnId}' n the logs etc...
            optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            optionsBuilder.EnableSensitiveDataLogging();

            var inMemoryDbContext = new DatabaseContext(optionsBuilder.Options);

            return inMemoryDbContext;
        }
    }
}
