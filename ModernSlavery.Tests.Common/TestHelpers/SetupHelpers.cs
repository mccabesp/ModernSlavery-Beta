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

            var badSicLog = new Mock<IRecordLogger>().Object;
            var manualChangeLog = new Mock<IRecordLogger>().Object;
            var registrationLog = new Mock<IRecordLogger>().Object;
            var submissionLog = new Mock<IRecordLogger>().Object;
            var searchLog = new Mock<IRecordLogger>().Object;
            var emailSendLog = new Mock<IRecordLogger>().Object;

            if (builder != null)
            {
                builder.RegisterInstance(Mock.Of<IFileRepository>()).SingleInstance();

                builder.RegisterInstance(badSicLog).Keyed<IRecordLogger>(Filenames.BadSicLog).SingleInstance();
                builder.RegisterInstance(manualChangeLog).Keyed<IRecordLogger>(Filenames.ManualChangeLog)
                    .SingleInstance();
                builder.RegisterInstance(registrationLog).Keyed<IRecordLogger>(Filenames.RegistrationLog)
                    .SingleInstance();
                builder.RegisterInstance(submissionLog).Keyed<IRecordLogger>(Filenames.SubmissionLog)
                    .SingleInstance();
                builder.RegisterInstance(searchLog).Keyed<IRecordLogger>(Filenames.SearchLog).SingleInstance();
                builder.RegisterInstance(emailSendLog).Keyed<IRecordLogger>(Filenames.EmailSendLog).SingleInstance();

                builder.RegisterInstance(Mock.Of<IUserLogger>()).SingleInstance();
                builder.RegisterInstance(Mock.Of<IRegistrationLogger>()).SingleInstance();
            }
        }
    }
}