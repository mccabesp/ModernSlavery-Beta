using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.WebUI
{
    public interface IMvcApplication
    {

        double SessionTimeOutMinutes { get; }

        ILogger Logger { get; }
        IQueue SendEmailQueue { get; set; }
        IQueue SendNotifyEmailQueue { get; set; }
        IQueue ExecuteWebjobQueue { get; }

        void Init();

    }

    public class MvcApplication : IMvcApplication
    {

        public static IContainer ContainerIoC;

        public MvcApplication(
            ILogger<MvcApplication> logger,
            IFileRepository fileRepository,
            ISearchRepository<EmployerSearchModel> searchRepository,
            [KeyFilter(QueueNames.SendEmail)] IQueue sendEmailQueue,
            [KeyFilter(QueueNames.SendNotifyEmail)]IQueue sendNotifyEmailQueue,
            [KeyFilter(QueueNames.ExecuteWebJob)] IQueue executeWebjobQueue
        )
        {
            Logger = logger;

            _fileRepository = fileRepository;

            SendEmailQueue = sendEmailQueue;
            SendNotifyEmailQueue = sendNotifyEmailQueue;
            ExecuteWebjobQueue = executeWebjobQueue;
        }

        private IFileRepository _fileRepository;
        public ILogger Logger { get; }
        public IQueue SendEmailQueue { get; set; }
        public IQueue SendNotifyEmailQueue { get; set; }
        public IQueue ExecuteWebjobQueue { get; }

        public double SessionTimeOutMinutes => Config.GetAppSetting("SessionTimeOut").ToInt32(20);

        public void Init()
        {
            //Ensure ShortCodes, SicCodes and SicSections exist on remote 
            Task.WaitAll(
                Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.ShortCodes, Global.DataPath),
                Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicCodes, Global.DataPath),
                Core.Classes.Extensions.PushRemoteFileAsync(_fileRepository, Filenames.SicSections, Global.DataPath)
            );
        }

    }
}
