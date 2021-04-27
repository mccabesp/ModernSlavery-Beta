using System;
using System.IO;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    public partial class AdminController : BaseController
    {
        /// <summary>
        ///     Refresh DATA in SCV files
        /// </summary>
        /// <param name="filePath"></param>
        private async Task UpdateFileAsync(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) throw new ArgumentNullException(nameof(command));

            var webjobName = command.BeforeFirst(":");
            if (string.IsNullOrWhiteSpace(webjobName)) throw new ArgumentNullException(nameof(command), "Missing webjobName");

            var filePath = command.AfterFirst(":");
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentNullException(nameof(command), "Missing filePath");

            //Mark the file as updating
            await SetFileUpdatingAsync(filePath);

            //trigger the update webjob
            await _adminService.ExecuteWebjobQueue.AddMessageAsync(new QueueWrapper($"command={webjobName}"));
        }

        /// <summary>
        ///     Checks if a file update has been triggered
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<bool> GetFileUpdatingAsync(string filePath)
        {
            filePath = filePath.ToLower();
            var updated = Session[$"FileUpdate:{filePath}"].ToDateTime();
            if (updated == DateTime.MinValue) return false;

            var changed = updated;
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                changed = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(filePath);

            return updated == changed;
        }

        /// <summary>
        ///     Marks a file as triggered for an update
        /// </summary>
        /// <param name="filePath"></param>
        private async Task SetFileUpdatingAsync(string filePath)
        {
            filePath = filePath.ToLower();
            if (await SharedBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
                Session[$"FileUpdate:{filePath}"] = await SharedBusinessLogic.FileRepository.GetLastWriteTimeAsync(filePath);
            else
                Session[$"FileUpdate:{filePath}"] = VirtualDateTime.Now;
        }
    }
}