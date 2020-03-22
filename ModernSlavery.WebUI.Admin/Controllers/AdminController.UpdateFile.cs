using System;
using System.IO;
using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.SharedKernel;
using ModernSlavery.WebUI.Shared.Controllers;

namespace ModernSlavery.WebUI.Admin.Controllers
{
    public partial class AdminController : BaseController
    {

        /// <summary>
        ///     Refresh DATA in SCV files
        /// </summary>
        /// <param name="filePath"></param>
        private async Task UpdateFileAsync(string filePath, string action = null)
        {
            string fileName = Path.GetFileName(filePath);

            if (fileName == Filenames.UnfinishedOrganisations)
            {
                throw new NotImplementedException(
                    $"Cannot execute {nameof(UpdateFileAsync)} on {fileName} as the code has not yet been implemented");
            }

            //Mark the file as updating
            await SetFileUpdatedAsync(filePath);

            //trigger the update webjob
            await AdminService.ExecuteWebjobQueue.AddMessageAsync(
                new QueueWrapper($"command=UpdateFile&filePath={filePath}&action={action}"));
        }

        /// <summary>
        ///     Checks if a file update has been triggered
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<bool> GetFileUpdatingAsync(string filePath)
        {
            filePath = filePath.ToLower();
            DateTime updated = Session[$"FileUpdate:{filePath}"].ToDateTime();
            if (updated == DateTime.MinValue)
            {
                return false;
            }

            DateTime changed = updated;
            if (await CommonBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
            {
                changed = await CommonBusinessLogic.FileRepository.GetLastWriteTimeAsync(filePath);
            }

            return updated == changed;
        }

        /// <summary>
        ///     Marks a file as triggered for an update
        /// </summary>
        /// <param name="filePath"></param>
        private async Task SetFileUpdatedAsync(string filePath)
        {
            filePath = filePath.ToLower();
            if (await CommonBusinessLogic.FileRepository.GetFileExistsAsync(filePath))
            {
                Session[$"FileUpdate:{filePath}"] = await CommonBusinessLogic.FileRepository.GetLastWriteTimeAsync(filePath);
            }
            else
            {
                Session[$"FileUpdate:{filePath}"] = VirtualDateTime.Now;
            }
        }

    }
}
