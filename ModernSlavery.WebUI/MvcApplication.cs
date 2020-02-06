using System.Threading.Tasks;
using Autofac;
using Autofac.Features.AttributeFilters;
using ModernSlavery.Core;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Extensions;
using ModernSlavery.Extensions.AspNetCore;
using Microsoft.Extensions.Logging;

namespace ModernSlavery.WebUI
{
    public interface IMvcApplication
    {

        double SessionTimeOutMinutes { get; }

        ILogger Logger { get; }
        IQueue SendEmailQueue { get; set; }
        IQueue SendNotifyEmailQueue { get; set; }
        IQueue ExecuteWebjobQueue { get; }

        Task InitAsync();

    }

    public class MvcApplication : IMvcApplication
    {

        public static IContainer ContainerIoC;

        public MvcApplication(
            ILogger<MvcApplication> logger,
            IFileRepository fileRepository,
            ISearchRepository<EmployerSearchModel> searchRepository,
            [KeyFilter(QueueNames.SendEmail)] IQueue sendEmailQueue,
            [KeyFilter(QueueNames.SendNotifyEmail)]
            IQueue sendNotifyEmailQueue,
            [KeyFilter(QueueNames.ExecuteWebJob)] IQueue executeWebjobQueue
        )
        {
            Logger = logger;

            Global.FileRepository = fileRepository;
            Global.SearchRepository = searchRepository;

            SendEmailQueue = sendEmailQueue;
            SendNotifyEmailQueue = sendNotifyEmailQueue;
            ExecuteWebjobQueue = executeWebjobQueue;
        }

        public ILogger Logger { get; }
        public IQueue SendEmailQueue { get; set; }
        public IQueue SendNotifyEmailQueue { get; set; }
        public IQueue ExecuteWebjobQueue { get; }

        public double SessionTimeOutMinutes => Config.GetAppSetting("SessionTimeOut").ToInt32(20);

        public async Task InitAsync()
        {
            //Copy AppData to remote file storage
            if (!Config.IsProduction())
            {
                await Core.Classes.Extensions.PushRemoteFileAsync(Global.FileRepository, Filenames.SicCodes, Global.DataPath);
                await Core.Classes.Extensions.PushRemoteFileAsync(Global.FileRepository, Filenames.SicSections, Global.DataPath);
            }
        }

    }
}
