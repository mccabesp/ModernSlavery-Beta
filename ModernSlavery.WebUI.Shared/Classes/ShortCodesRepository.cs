using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using ModernSlavery.SharedKernel;
using ModernSlavery.WebUI.Shared.Models;

namespace ModernSlavery
{
    public class ShortCodesRepository: IShortCodesRepository
    {
        public ShortCodesRepository(IFileRepository fileRepository)
        {
            FileRepository = fileRepository;
        }

        private readonly IFileRepository FileRepository;
        #region Properties

        private DateTime _ShortCodesLoaded;
        internal DateTime _ShortCodesLastLoaded;

        private List<ShortCodeModel> _ShortCodes;

        //Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        private readonly SemaphoreSlim _ShortCodesLock = new SemaphoreSlim(1, 1);

        public async Task<List<ShortCodeModel>> GetAllShortCodesAsync()
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await _ShortCodesLock.WaitAsync();
            try
            {
                if (_ShortCodes == null || _ShortCodesLastLoaded.AddMinutes(5) < VirtualDateTime.Now)
                {
                    List<ShortCodeModel> orgs = await LoadIfNewerAsync();
                    if (orgs != null)
                    {
                        _ShortCodes = orgs;
                    }

                    _ShortCodesLastLoaded = VirtualDateTime.Now;
                }

                return _ShortCodes;
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                _ShortCodesLock.Release();
            }
        }

        public async Task ClearAllShortCodesAsync()
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await _ShortCodesLock.WaitAsync();
            try
            {
                _ShortCodesLoaded = DateTime.MinValue;
                _ShortCodesLastLoaded = DateTime.MinValue;
                _ShortCodes = null;
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                _ShortCodesLock.Release();
            }
        }

        public async Task<List<ShortCodeModel>> LoadIfNewerAsync()
        {
            string shortCodesPath = Path.Combine(Global.DataPath, Filenames.ShortCodes);
            bool fileExists = await FileRepository.GetFileExistsAsync(shortCodesPath);

            if (!fileExists)
            {
                return null;
            }

            DateTime newloadTime = fileExists ? await FileRepository.GetLastWriteTimeAsync(shortCodesPath) : DateTime.MinValue;

            if (_ShortCodesLoaded > DateTime.MinValue && newloadTime <= _ShortCodesLoaded)
            {
                return null;
            }

            string orgs = fileExists ? await FileRepository.ReadAsync(shortCodesPath) : null;
            if (string.IsNullOrWhiteSpace(orgs))
            {
                throw new Exception($"No content not load '{shortCodesPath}'");
            }

            _ShortCodesLoaded = newloadTime;

            List<ShortCodeModel> list = await FileRepository.ReadCSVAsync<ShortCodeModel>(shortCodesPath);
            if (!list.Any())
            {
                throw new Exception($"No records found in '{shortCodesPath}'");
            }

            list = list.OrderBy(c => c.ShortCode).ToList();

            return list;
        }

        #endregion

    }
}
