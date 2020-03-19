using Autofac;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.SharedKernel;
using Moq;

namespace ModernSlavery.Tests.Common.TestHelpers
{
    public static class SetupHelpers
    {
        //TODO: Possible seperate this method for WebUI and Webjobs?
        public static void SetupMockLogRecordGlobals(ContainerBuilder builder = null)
        {
            // Used by WebUI

            var badSicLog = new Mock<ILogRecordLogger>().Object;
            var manualChangeLog = new Mock<ILogRecordLogger>().Object;
            var registrationLog = new Mock<ILogRecordLogger>().Object;
            var submissionLog = new Mock<ILogRecordLogger>().Object;
            var searchLog = new Mock<ILogRecordLogger>().Object;
            var emailSendLog = new Mock<ILogRecordLogger>().Object;

            if (builder != null)
            {
                builder.RegisterInstance(Mock.Of<IFileRepository>()).SingleInstance();

                builder.RegisterInstance(badSicLog).Keyed<ILogRecordLogger>(Filenames.BadSicLog).SingleInstance();
                builder.RegisterInstance(manualChangeLog).Keyed<ILogRecordLogger>(Filenames.ManualChangeLog)
                    .SingleInstance();
                builder.RegisterInstance(registrationLog).Keyed<ILogRecordLogger>(Filenames.RegistrationLog)
                    .SingleInstance();
                builder.RegisterInstance(submissionLog).Keyed<ILogRecordLogger>(Filenames.SubmissionLog)
                    .SingleInstance();
                builder.RegisterInstance(searchLog).Keyed<ILogRecordLogger>(Filenames.SearchLog).SingleInstance();
                builder.RegisterInstance(emailSendLog).Keyed<ILogRecordLogger>(Filenames.EmailSendLog).SingleInstance();

                builder.RegisterInstance(Mock.Of<IUserLogRecord>()).SingleInstance();
                builder.RegisterInstance(Mock.Of<IRegistrationLogRecord>()).SingleInstance();
            }
        }
    }
}