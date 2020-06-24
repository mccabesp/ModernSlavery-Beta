using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Infrastructure.Database;
using ModernSlavery.Infrastructure.Database.Classes;
using Moq;
using NUnit.Framework;

namespace ModernSlavery.Tests.Common.Classes
{
    public static class AutoFacHelpers
    {
        public static IContainer DIContainer { get; private set; }

        /// <summary>
        ///     Resolves to a Mock object for the of specified instance and resolves the constructor arguments using the container
        ///     context
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="context"></param>
        /// <param name="callBase">
        ///     Whether the base member virtual implementation will be called for mocked classes if no setup is
        ///     matched
        /// </param>
        /// <param name="ctorArgs"></param>
        /// <returns></returns>
        public static Mock<TInstance> ResolveAsMock<TInstance>(this IComponentContext context, bool callBase,
            params Type[] ctorArgs)
            where TInstance : class
        {
            var r = new List<object>();
            foreach (var t in ctorArgs) r.Add(context.Resolve(t));

            var mock = new Mock<TInstance>(r.ToArray());
            mock.CallBase = callBase;
            return mock;
        }


        /// <summary>
        ///     Registers an InMemory SQLRespository and populates with entities
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dbObjects"></param>
        public static void RegisterInMemoryTestDatabase(this ContainerBuilder builder, params object[] dbObjects)
        {
            var dbContext = CreateInMemoryTestDatabase(dbObjects);
            builder.Register(c => new SqlRepository(dbContext)).As<IDataRepository>().InstancePerLifetimeScope();
        }

        public static DatabaseContext CreateInMemoryTestDatabase(params object[] dbObjects)
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

            return dbContext;
        }
    }
}