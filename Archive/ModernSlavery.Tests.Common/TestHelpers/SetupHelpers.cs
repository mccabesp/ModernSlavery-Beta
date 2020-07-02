using Autofac;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.SharedKernel;
using Moq;

namespace ModernSlavery.Tests.Common.TestHelpers
{
    public static class SetupHelpers
    {
        //TODO: Possible seperate this method for WebUI and Webjobs?
        public static void SetupMockLogRecordGlobals(ContainerBuilder builder = null)
        {
            // Used by WebUI

            var badSicLog = new Mock<IAuditLogger>().Object;
            var manualChangeLog = new Mock<IAuditLogger>().Object;
            var registrationLog = new Mock<IAuditLogger>().Object;
            var submissionLog = new Mock<IAuditLogger>().Object;
            var searchLog = new Mock<IAuditLogger>().Object;
            var emailSendLog = new Mock<IAuditLogger>().Object;

            if (builder != null)
            {
                builder.RegisterInstance(Mock.Of<IFileRepository>()).SingleInstance();

                builder.RegisterInstance(badSicLog).Keyed<IAuditLogger>(Filenames.BadSicLog).SingleInstance();
                builder.RegisterInstance(manualChangeLog).Keyed<IAuditLogger>(Filenames.ManualChangeLog)
                    .SingleInstance();
                builder.RegisterInstance(registrationLog).Keyed<IAuditLogger>(Filenames.RegistrationLog)
                    .SingleInstance();
                builder.RegisterInstance(submissionLog).Keyed<IAuditLogger>(Filenames.SubmissionLog)
                    .SingleInstance();
                builder.RegisterInstance(searchLog).Keyed<IAuditLogger>(Filenames.SearchLog).SingleInstance();
                builder.RegisterInstance(emailSendLog).Keyed<IAuditLogger>(Filenames.EmailSendLog).SingleInstance();

                builder.RegisterInstance(Mock.Of<IUserLogger>()).SingleInstance();
                builder.RegisterInstance(Mock.Of<IRegistrationLogger>()).SingleInstance();
            }
        }
    }
}