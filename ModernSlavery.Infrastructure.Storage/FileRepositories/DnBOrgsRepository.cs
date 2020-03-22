using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Core.SharedKernel;

namespace ModernSlavery.Infrastructure.Storage.FileRepositories
{
    public class DnBOrgsRepository : IDnBOrgsRepository
    {
        private readonly string DataPath;

        private readonly IQueue ExecuteWebjobQueue;
        private readonly IFileRepository FileRepository;

        public DnBOrgsRepository([KeyFilter(QueueNames.ExecuteWebJob)] IQueue executeWebjobQueue,
            IFileRepository fileRepository, string dataPath)
        {
            ExecuteWebjobQueue = executeWebjobQueue;
            FileRepository = fileRepository;
            DataPath = dataPath;
        }

        public async Task UploadAsync(List<DnBOrgsModel> newOrgs)
        {
            if (newOrgs == null || newOrgs.Count == 0) throw new ArgumentNullException(nameof(newOrgs));

            //Check new file
            var count = newOrgs.Count(o => string.IsNullOrWhiteSpace(o.DUNSNumber));
            if (count > 0)
                throw new Exception($"There are {count} organisations in the D&B file without a DUNS number");

            var allDnBOrgs = await GetAllDnBOrgsAsync();
            //Check for no organisation name
            count = allDnBOrgs == null ? 0 : allDnBOrgs.Count(o => string.IsNullOrWhiteSpace(o.OrganisationName));
            if (count > 0) throw new Exception($"There are {count} organisations with no OrganisationName detected.");

            //Check for no addresses
            count = allDnBOrgs == null ? 0 : allDnBOrgs.Count(o => !o.IsValidAddress());
            if (count > 0)
            {
                await ExecuteWebjobQueue.AddMessageAsync(new QueueWrapper("CompaniesHouseCheck"));
                throw new Exception(
                    $"There are {count} organisations with invalid addresses (i.e., no UK postcode or pobox, or any foreign address field).");
            }

            //Check for duplicate DUNS
            count = newOrgs.Count() - newOrgs.Select(o => o.DUNSNumber).Distinct().Count();
            if (count > 0) throw new Exception($"There are {count} duplicate DUNS numbers detected");

            //Check for duplicate employer references
            var employerReferences =
                newOrgs.Where(o => !string.IsNullOrWhiteSpace(o.EmployerReference)).Select(o => o.EmployerReference);
            count = employerReferences.Count() - employerReferences.Distinct().Count();
            if (count > 0) throw new Exception($"There are {count} duplicate EmployerReferences detected");

            //Fix Company Number
            Parallel.ForEach(
                newOrgs.Where(o => !string.IsNullOrWhiteSpace(o.CompanyNumber)),
                dnbOrg =>
                {
                    if (dnbOrg.CompanyNumber.IsNumber()) dnbOrg.CompanyNumber = dnbOrg.CompanyNumber.PadLeft(8, '0');
                });

            //Check for duplicate company numbers
            var companyNumbers =
                newOrgs.Where(o => !string.IsNullOrWhiteSpace(o.CompanyNumber)).Select(o => o.CompanyNumber);
            count = companyNumbers.Count() - companyNumbers.Distinct().Count();
            if (count > 0) throw new Exception($"There are {count} duplicate CompanyNumbers detected");

            //Copy all old settings to new 
            if (allDnBOrgs != null)
                Parallel.ForEach(
                    newOrgs,
                    newOrg =>
                    {
                        var oldOrg = allDnBOrgs.FirstOrDefault(o => o.DUNSNumber == newOrg.DUNSNumber);
                        //Make sure missing old orgs are copied accross
                        if (oldOrg == null) return;

                        if (string.IsNullOrWhiteSpace(newOrg.EmployerReference) &&
                            !string.IsNullOrWhiteSpace(oldOrg.EmployerReference))
                            newOrg.EmployerReference = oldOrg.EmployerReference;
                    });
        }

        public async Task ImportAsync(IDataRepository dataRepository, User currentUser)
        {
            await ClearAllDnBOrgsAsync(); //Must clear first 

            var allDnBOrgs = await GetAllDnBOrgsAsync();

            //Check for duplicate DUNS
            var count = allDnBOrgs.Count() - allDnBOrgs.Select(o => o.DUNSNumber).Distinct().Count();
            if (count > 0) throw new Exception($"There are {count} duplicate DUNS numbers detected");

            //Check for no addresses
            count = allDnBOrgs.Count(o => !o.IsValidAddress());
            if (count > 0)
            {
                await ExecuteWebjobQueue.AddMessageAsync(new QueueWrapper("CompaniesHouseCheck"));
                throw new Exception(
                    $"There are {count} organisations with invalid addresses (i.e., no UK postcode or pobox, or any foreign address field).");
            }


            //Check for duplicate employer references
            var employerReferences =
                allDnBOrgs.Where(o => !string.IsNullOrWhiteSpace(o.EmployerReference)).Select(o => o.EmployerReference);
            count = employerReferences.Count() - employerReferences.Distinct().Count();
            if (count > 0) throw new Exception($"There are {count} duplicate EmployerReferences detected");

            //Fix Company Number
            Parallel.ForEach(
                allDnBOrgs.Where(o => !string.IsNullOrWhiteSpace(o.CompanyNumber)),
                dnbOrg =>
                {
                    if (dnbOrg.CompanyNumber.IsNumber()) dnbOrg.CompanyNumber = dnbOrg.CompanyNumber.PadLeft(8, '0');
                });

            //Check for duplicate company numbers
            var companyNumbers =
                allDnBOrgs.Where(o => !string.IsNullOrWhiteSpace(o.CompanyNumber)).Select(o => o.CompanyNumber);
            count = companyNumbers.Count() - companyNumbers.Distinct().Count();
            if (count > 0) throw new Exception($"There are {count} duplicate CompanyNumbers detected");

            //Check companies have been updated
            count = allDnBOrgs.Count(
                o => !string.IsNullOrWhiteSpace(o.CompanyNumber)
                     && !o.GetIsDissolved()
                     && (o.StatusCheckedDate == null || o.StatusCheckedDate.Value.AddMonths(1) < VirtualDateTime.Now));
            if (count > 0)
            {
                await ExecuteWebjobQueue.AddMessageAsync(new QueueWrapper("CompaniesHouseCheck"));
                throw new Exception(
                    $"There are {count} active companies who have not been checked with companies house within the last month. A recheck has now been initiated please try again in 5 minutes.");
            }

            //Count records requiring import
            count = allDnBOrgs.Count(
                o => !o.GetIsDissolved()
                     && (o.ImportedDate == null || string.IsNullOrWhiteSpace(o.CompanyNumber) ||
                         o.ImportedDate < o.StatusCheckedDate));
            if (count == 0) throw new Exception("There are no records requiring import");

            //Execute the webjob
            await ExecuteWebjobQueue.AddMessageAsync(
                new QueueWrapper("command=DnBImport&currentUserId=" + currentUser.UserId));
        }

        public async Task<List<DnBOrgsModel>> GetAllDnBOrgsAsync()
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await _DnBOrgsLock.WaitAsync();
            try
            {
                if (_DnBOrgs == null || _DnBOrgsLastLoaded.AddMinutes(5) < VirtualDateTime.Now)
                {
                    var orgs = await LoadIfNewerAsync();
                    if (orgs != null) _DnBOrgs = orgs;

                    _DnBOrgsLastLoaded = VirtualDateTime.Now;
                }

                return _DnBOrgs;
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                _DnBOrgsLock.Release();
            }
        }

        public async Task ClearAllDnBOrgsAsync()
        {
            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await _DnBOrgsLock.WaitAsync();
            try
            {
                _DnBOrgsLoaded = DateTime.MinValue;
                _DnBOrgsLastLoaded = DateTime.MinValue;
                _DnBOrgs = null;
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                _DnBOrgsLock.Release();
            }
        }

        public async Task<List<DnBOrgsModel>> LoadIfNewerAsync()
        {
            var dnbOrgsPath = Path.Combine(DataPath, Filenames.DnBOrganisations());
            var fileExists = await FileRepository.GetFileExistsAsync(dnbOrgsPath);

            //Copy the previous years if no current year
            if (!fileExists)
            {
                var dnbOrgsPathPrevious = Path.Combine(DataPath, Filenames.PreviousDnBOrganisations());
                if (await FileRepository.GetFileExistsAsync(dnbOrgsPathPrevious))
                {
                    await FileRepository.WriteAsync(dnbOrgsPath,
                        await FileRepository.ReadBytesAsync(dnbOrgsPathPrevious));
                    fileExists = await FileRepository.GetFileExistsAsync(dnbOrgsPath);
                }
            }

            if (!fileExists) return null;

            var newloadTime = fileExists ? await FileRepository.GetLastWriteTimeAsync(dnbOrgsPath) : DateTime.MinValue;

            if (_DnBOrgsLoaded > DateTime.MinValue && newloadTime <= _DnBOrgsLoaded) return null;

            var orgs = fileExists ? await FileRepository.ReadAsync(dnbOrgsPath) : null;
            if (string.IsNullOrWhiteSpace(orgs)) throw new Exception($"No content not load '{dnbOrgsPath}'");

            _DnBOrgsLoaded = newloadTime;

            var list = await FileRepository.ReadCSVAsync<DnBOrgsModel>(dnbOrgsPath);
            if (list.Count < 1) throw new Exception($"No records found in '{dnbOrgsPath}'");

            foreach (var org in list.OrderBy(o => o.OrganisationName))
                if (org.CompanyNumber.IsNumber())
                    org.CompanyNumber = org.CompanyNumber.PadLeft(8, '0');

            return list;
        }


        #region Properties

        private DateTime _DnBOrgsLoaded;
        internal DateTime _DnBOrgsLastLoaded;
        private List<DnBOrgsModel> _DnBOrgs;
        private readonly SemaphoreSlim _DnBOrgsLock = new SemaphoreSlim(1, 1);

        #endregion
    }
}