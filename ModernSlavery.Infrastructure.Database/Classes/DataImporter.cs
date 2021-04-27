using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Classes.StatementTypeIndexes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Database.Migrations;

namespace ModernSlavery.Infrastructure.Database.Classes
{
    public class DataImporter : IDataImporter
    {
        private readonly ILogger _logger;
        private readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        private readonly IDbContext _dbContext;
        private readonly IFileRepository _fileRepository;
        private readonly SectorTypeIndex _sectorTypeIndex;
        private static readonly int? CommandTimeout = 500;

        public DataImporter(ILogger<DataImporter> logger, SharedOptions sharedOptions, IDbContext dbContext, IDataRepository dataRepository, IFileRepository fileRepository, SectorTypeIndex sectorTypeIndex)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _sectorTypeIndex = sectorTypeIndex ?? throw new ArgumentNullException(nameof(sectorTypeIndex));
        }

        #region Seed the SIC Codes and Categories
        public async Task<int> ImportSICSectionsAsync(bool importWhenAny = false)
        {
            var filepath = Path.Combine(_sharedOptions.AppDataPath, Filenames.SicSections);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.AppDataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.AppDataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<SicSection>();
            if (!importWhenAny && databaseRecords.Any()) return 0;

            var fileRecords = await _fileRepository.ReadCSVAsync<SicSection>(filepath, false).ConfigureAwait(false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            int insertCount = 0;
            int updateCount = 0;

            foreach (var fileRecord in fileRecords)
            {
                var oldRecord = _dataRepository.Get<SicSection>(fileRecord.SicSectionId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = fileRecord;
                    isnew = true;
                }

                oldRecord.Description = fileRecord.Description;
                if (isnew)
                {
                    _dataRepository.Insert(oldRecord);
                    insertCount++;
                }
                else
                {
                    oldRecord.Created = VirtualDateTime.Now;
                    _dataRepository.Update(oldRecord);
                    updateCount++;
                }
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<SicSection>().ToList().Where(s => !s.SicCodes.Any() && !fileRecords.Any(n => n.SicSectionId == s.SicSectionId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation($"{nameof(ImportSICSectionsAsync)}: Inserted {insertCount} and updated {updateCount} of {fileRecords.Count} records");

            return insertCount + updateCount;
        }

        public async Task<int> ImportSICCodesAsync(bool importWhenAny = false)
        {
            var filepath = Path.Combine(_sharedOptions.AppDataPath, Filenames.SicCodes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.AppDataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.AppDataPath).ConfigureAwait(false);

            var databaseRecords = await _dataRepository.GetAll<SicCode>().ToListAsync().ConfigureAwait(false);
            if (!importWhenAny && databaseRecords.Any()) return 0;

            var fileRecords = await _fileRepository.ReadCSVAsync<SicCode>(filepath, false).ConfigureAwait(false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            int insertCount = 0;
            int updateCount = 0;

            foreach (var fileRecord in fileRecords)
            {
                var oldRecord = _dataRepository.Get<SicCode>(fileRecord.SicCodeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = fileRecord;
                    isnew = true;
                }

                oldRecord.SicSection = _dataRepository.Get<SicSection>(fileRecord.SicSectionId);
                oldRecord.SicSectionId = fileRecord.SicSectionId;
                oldRecord.SicCodeId = fileRecord.SicCodeId;

                oldRecord.Description = fileRecord.Description;
                
                if (isnew)
                {
                    _dataRepository.Insert(oldRecord);
                    insertCount++;
                }
                else
                {
                    oldRecord.Created = VirtualDateTime.Now;
                    _dataRepository.Update(oldRecord);
                    updateCount++;
                }
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<SicCode>().ToList().Where(s => !s.OrganisationSicCodes.Any() && !fileRecords.Any(n => n.SicCodeId == s.SicCodeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
            _logger.LogInformation($"{nameof(ImportSICCodesAsync)}: Inserted {insertCount} and updated {updateCount} of {fileRecords.Count} records");

            return insertCount + updateCount;
        }
        #endregion

        public async Task EnsureSystemUserExistsAsync()
        {
            //Ensure we have a system user
            if (_dataRepository.Get<User>((long)-1) == null)
            {
                await _dbContext.GetDatabase().ExecuteSqlRawAsync(AddSystemUser.AddSystemUserSql);
            }
        }

        #region Seed Statement types
        public async Task<int> ImportStatementSectorTypesAsync(bool importWhenAny = false)
        {
            var filepath = Path.Combine(_sharedOptions.AppDataPath, Filenames.StatementSectorTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.AppDataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.AppDataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<StatementSectorType>();
            if (!importWhenAny && databaseRecords.Any()) return 0;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementSectorType>(filepath, false).ConfigureAwait(false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            int insertCount=0;
            int updateCount=0;
            foreach (var newRecord in newRecords)
            {
                var oldRecord = _dataRepository.Get<StatementSectorType>(newRecord.StatementSectorTypeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = newRecord;
                    isnew = true;
                }

                oldRecord.Description = newRecord.Description;
                if (isnew)
                {
                    _dataRepository.Insert(oldRecord);
                    insertCount++;
                }
                else
                {
                    oldRecord.Created = VirtualDateTime.Now;
                    _dataRepository.Update(oldRecord);
                    updateCount++;
                }
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<StatementSectorType>().ToList().Where(s => !s.StatementSectors.Any() && !newRecords.Any(n => n.StatementSectorTypeId == s.StatementSectorTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);

            //Reload the new sector types into memory
            _sectorTypeIndex.Load();

            _logger.LogInformation($"{nameof(ImportStatementSectorTypesAsync)}: Inserted {insertCount} and updated {updateCount} of {newRecords.Count} records");

            return insertCount+updateCount;
        }
        #endregion

        #region Seed Organisations
        /// <summary>
        ///     //Seed the private organisations in the database
        /// </summary>
        /// <param name="importWhenAny">When true always imports. When false only when no private organisation records in db</param>
        public async Task<int> ImportPrivateOrganisationsAsync(long userId, int maxRecords = 0, bool importWhenAny = false, bool throwWhenExists = false)
        {
            return await ImportOrganisationsAsync(Filenames.ImportPrivateOrganisations, SectorTypes.Private, userId, maxRecords, importWhenAny, throwWhenExists).ConfigureAwait(false);
        }

        /// <summary>
        ///     //Seed the public organisations in the database
        /// </summary>
        /// <param name="importWhenAny">When true always imports. When false only when no public organisation records in db</param>
        public async Task<int> ImportPublicOrganisationsAsync(long userId, int maxRecords = 0, bool importWhenAny = false, bool throwWhenExists=false)
        {
            return await ImportOrganisationsAsync(Filenames.ImportPublicOrganisations, SectorTypes.Public, userId, maxRecords, importWhenAny, throwWhenExists).ConfigureAwait(false);
        }

        private async Task<int> ImportOrganisationsAsync(string fileName, SectorTypes sectorType, long userId, int maxRecords = 0, bool importWhenAny = false, bool throwWhenExists = false)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (sectorType == SectorTypes.Unknown) throw new ArgumentOutOfRangeException(nameof(sectorType));
            if (userId == 0) throw new ArgumentOutOfRangeException(nameof(userId), "UserId cannot be 0");

            var filepath = Path.Combine(_sharedOptions.AppDataPath, fileName);

            //Create the directory if it doesnt exist
            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.AppDataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.AppDataPath).ConfigureAwait(false);

            //Dont import unless forced when there are existing records
            var orgs = await _dataRepository.ToListAsync<Organisation>(o => o.SectorType == sectorType).ConfigureAwait(false);

            if (!importWhenAny && orgs.Any()) return 0;

            var coHoOrgs = await _dataRepository.ToListAsync<Organisation>(o => o.CompanyNumber != null).ConfigureAwait(false);
            var noCoHoOrgs = await _dataRepository.ToListAsync<Organisation>(o => o.CompanyNumber == null).ConfigureAwait(false);
            //Make sure all addresses are loaded
            if (sectorType == SectorTypes.Public) noCoHoOrgs.SelectMany(o => o.OrganisationAddresses).ToList();

            //Get the imported records
            var newRecords = await _fileRepository.ReadCSVAsync<ImportOrganisationModel>(filepath, false).ConfigureAwait(false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Limit the number of records imported
            if (maxRecords > 0) newRecords = newRecords.Take(maxRecords).ToList();

            var processedNames = new ConcurrentSet<string>();
            var processedNumbers = new ConcurrentSet<string>();

            var allSicCodes = _dataRepository.GetAll<SicCode>().Select(s => s.SicCodeId).ToSortedSet();

            var exceptions = new ConcurrentBag<Exception>();
            var bulkOrganisations = new ConcurrentBag<Organisation>();
            var bulkOrganisationNames = new ConcurrentBag<OrganisationName>();
            var bulkOrganisationStatuses = new ConcurrentBag<OrganisationStatus>();
            var bulkAddresses = new ConcurrentBag<OrganisationAddress>();
            var bulkAddressStatuses = new ConcurrentBag<AddressStatus>();
            var bulkOrganisationSicCodes = new ConcurrentBag<OrganisationSicCode>();

            //Add or update the new records
            var created = VirtualDateTime.Now;

            if (Debugger.IsAttached)
                foreach (var newRecord in newRecords) await AddRecord(newRecord);
            else
                Parallel.ForEach(newRecords, async newRecord => await AddRecord(newRecord));

            async Task AddRecord(ImportOrganisationModel newRecord)
            {
                try
                {
                    //Check organisation name and length
                    newRecord.OrganisationName = string.IsNullOrWhiteSpace(newRecord.OrganisationName) ? null : newRecord.OrganisationName.TrimNonLettersOrDigits();
                    if (string.IsNullOrWhiteSpace(newRecord.OrganisationName))
                        throw new Exception($"Empty organisation name '{fileName}'");
                    else if (newRecord.OrganisationName.Length > 160) 
                        throw new Exception($"{nameof(newRecord.OrganisationName)} exceeds 160 characters for organisation '{newRecord.OrganisationName}'");

                    //Check company number and length
                    newRecord.CompanyNumber = string.IsNullOrWhiteSpace(newRecord.CompanyNumber) ? null : newRecord.CompanyNumber.TrimNonLettersOrDigits();
                    if (!string.IsNullOrWhiteSpace(newRecord.CompanyNumber) && newRecord.CompanyNumber.Length > 10) throw new Exception($"{nameof(newRecord.CompanyNumber)} exceeds 10 characters for organisation '{newRecord.OrganisationName}'");

                    //Check the sector is as expected in the file
                    if (string.IsNullOrWhiteSpace(newRecord.Sector))
                        newRecord.Sector = sectorType.ToString();
                    else if (!sectorType.ToString().EqualsI(newRecord.Sector))
                        throw new Exception($"Invalid sector '{newRecord.Sector}' in {sectorType.ToString().ToLower()} file '{fileName}'");

                    //Check the sic codes
                    if (string.IsNullOrWhiteSpace(newRecord.SICCode) && string.IsNullOrWhiteSpace(newRecord.CompanyNumber))
                        throw new Exception($"Missing SicCode for organisation '{newRecord.OrganisationName}'");

                    //Check post code
                    if (string.IsNullOrWhiteSpace(newRecord.PostCode))
                    {
                        if (string.IsNullOrWhiteSpace(newRecord.CompanyNumber)) throw new Exception($"Missing PostCode and CompanyNumber for organisation '{newRecord.OrganisationName}'");
                    }
                    else if (newRecord.PostCode.Length > 20)
                        throw new Exception($"{nameof(newRecord.PostCode)} exceeds 20 characters for organisation '{newRecord.OrganisationName}'");

                    //Check address field length
                    var newAddress = newRecord.GetAddressString();
                    var hasAddress = !string.IsNullOrWhiteSpace(newAddress);

                    if (hasAddress)
                    {
                        if (!string.IsNullOrWhiteSpace(newRecord.Address1) && newRecord.Address1.Length > 100)
                            throw new Exception($"{nameof(newRecord.Address1)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'");
                        if (!string.IsNullOrWhiteSpace(newRecord.Address2) && newRecord.Address2.Length > 100)
                            throw new Exception($"{nameof(newRecord.Address2)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'");
                        if (!string.IsNullOrWhiteSpace(newRecord.Address3) && newRecord.Address3.Length > 100)
                            throw new Exception($"{nameof(newRecord.Address3)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'");
                        if (!string.IsNullOrWhiteSpace(newRecord.TownCity) && newRecord.TownCity.Length > 100)
                            throw new Exception($"{nameof(newRecord.TownCity)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'");
                        if (!string.IsNullOrWhiteSpace(newRecord.County) && newRecord.County.Length > 100)
                            throw new Exception($"{nameof(newRecord.County)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'");
                        if (!string.IsNullOrWhiteSpace(newRecord.Country) && newRecord.Country.Length > 100)
                            throw new Exception($"{nameof(newRecord.Country)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'");
                    }

                    var newSicCodes = newRecord.SICCode.SplitI(";,: ".ToCharArray()).Select(c => c.ToInt32()).ToHashSet();
                    if (sectorType == SectorTypes.Public) newSicCodes.Add(1);
                    var badSicCodes = newSicCodes.Except(allSicCodes);
                    if (badSicCodes.Any())
                    {
                        if (!string.IsNullOrWhiteSpace(newRecord.CompanyNumber))
                            newSicCodes = newSicCodes.Except(badSicCodes).ToHashSet();
                        else
                            throw new Exception($"Invalid SicCodes '{badSicCodes.ToDelimitedString()}' for organisation '{newRecord.OrganisationName}'");
                    }

                    //Try and get the existing organisation
                    Organisation org = null;
                    if (!string.IsNullOrWhiteSpace(newRecord.CompanyNumber))
                    {
                        newRecord.CompanyNumber = newRecord.CompanyNumber.IsNumber() ? newRecord.CompanyNumber.PadLeft(8, '0') : newRecord.CompanyNumber;
                        if (processedNumbers.Contains(newRecord.CompanyNumber))
                            throw new Exception($"Duplicate company number '{newRecord.CompanyNumber}' ");
                        else
                            org = coHoOrgs.SingleOrDefault(o => o.CompanyNumber == newRecord.CompanyNumber);
                    }
                    else
                    {
                        if (processedNames.Contains(newRecord.OrganisationName))
                            throw new Exception($"Duplicate organisation '{newRecord.OrganisationName}' ");
                        else if (sectorType == SectorTypes.Public)
                            org = noCoHoOrgs.SingleOrDefault(o => o.OrganisationName.EqualsI(newRecord.OrganisationName));
                        else
                            throw new Exception($"Attempt to import private organisation '{newRecord.OrganisationName}' with missing CompanyNumber");
                    }

                    if (org != null)
                    {
                        if (throwWhenExists)throw new Exception($"Organisation {newRecord.OrganisationName}:{newRecord.CompanyNumber}:{newRecord.Sector}:{newAddress}: already exists in database as {org.OrganisationName}:{org.CompanyNumber}:{org.SectorType}");
                        return;
                    }

                    org = new Organisation() {
                        SectorType = sectorType,
                        OrganisationName = newRecord.OrganisationName,
                        CompanyNumber = string.IsNullOrWhiteSpace(newRecord.CompanyNumber) ? null : newRecord.CompanyNumber,
                    };

                    var orgStatus = org.SetStatus(OrganisationStatuses.Active, userId);
                    if (orgStatus != null) bulkOrganisationStatuses.Add(orgStatus);

                    //Add the name
                    var orgName = new OrganisationName { Organisation = org, Name = org.OrganisationName, Source = "External" };
                    org.OrganisationNames.Add(orgName);
                    bulkOrganisationNames.Add(orgName);

                    //Add the new address
                    if (hasAddress)
                    {
                        var address = new OrganisationAddress {
                            Organisation = org,
                            CreatedByUserId = userId,
                            Address1 = string.IsNullOrWhiteSpace(newRecord.Address1) ? null : newRecord.Address1,
                            Address2 = string.IsNullOrWhiteSpace(newRecord.Address2) ? null : newRecord.Address2,
                            Address3 = string.IsNullOrWhiteSpace(newRecord.Address3) ? null : newRecord.Address3,
                            TownCity = string.IsNullOrWhiteSpace(newRecord.TownCity) ? null : newRecord.TownCity,
                            County = string.IsNullOrWhiteSpace(newRecord.County) ? null : newRecord.County,
                            Country = string.IsNullOrWhiteSpace(newRecord.Country) ? null : newRecord.Country,
                            PostCode = string.IsNullOrWhiteSpace(newRecord.PostCode) ? null : newRecord.PostCode,
                            Source = "External"
                        };
                        address.Trim();
                        var addressStatus = address.SetStatus(AddressStatuses.Active, userId);
                        if (addressStatus != null) bulkAddressStatuses.Add(addressStatus);
                        org.OrganisationAddresses.Add(address);
                        bulkAddresses.Add(address);
                    }

                    //Add the new sicCode
                    foreach (var sicCodeId in newSicCodes)
                    {
                        var sicCode = new OrganisationSicCode { Organisation = org, SicCodeId = sicCodeId, Source = "External" };
                        org.OrganisationSicCodes.Add(sicCode);
                        bulkOrganisationSicCodes.Add(sicCode);
                    }

                    //Remember the org reference
                    if (!string.IsNullOrWhiteSpace(newRecord.CompanyNumber))
                        processedNumbers.Add(newRecord.CompanyNumber);
                    else
                        processedNames.Add(org.OrganisationName);

                    //Save toe org to the bulk list
                    bulkOrganisations.Add(org);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            await _dataRepository.ExecuteTransactionAsync(async () => {
                try
                {
                    if (bulkOrganisations.Any())
                    {
                        _dataRepository.BeginTransaction();
                        //We need to set a dummy id to preserver insert order so we can retrieve ids 
                        long i = -10 - bulkOrganisations.Count;
                        bulkOrganisations.ForEach(org => org.OrganisationId = i++);
                        await _dataRepository.BulkInsertAsync(bulkOrganisations, true, timeout: CommandTimeout).ConfigureAwait(false);
                        Parallel.ForEach(bulkOrganisations, org => {
                            org.OrganisationStatuses.ForEach(s => s.OrganisationId = org.OrganisationId);
                            org.OrganisationSicCodes.ForEach(s => s.OrganisationId = org.OrganisationId);
                            org.OrganisationNames.ForEach(s => s.OrganisationId = org.OrganisationId);
                            org.OrganisationAddresses.ForEach(s => s.OrganisationId = org.OrganisationId);
                        });
                        if (bulkOrganisationStatuses.Any()) await _dataRepository.BulkInsertAsync(bulkOrganisationStatuses, timeout: CommandTimeout).ConfigureAwait(false);
                        if (bulkOrganisationSicCodes.Any()) await _dataRepository.BulkInsertAsync(bulkOrganisationSicCodes, timeout: CommandTimeout).ConfigureAwait(false);
                        if (bulkOrganisationNames.Any()) await _dataRepository.BulkInsertAsync(bulkOrganisationNames, timeout: CommandTimeout).ConfigureAwait(false);

                        //We need to set a dummy id to preserver insert order so we can retrieve ids 
                        i = -10 - bulkAddresses.Count;
                        bulkAddresses.ForEach(address => address.AddressId = i++);
                        if (bulkAddresses.Any()) await _dataRepository.BulkInsertAsync(bulkAddresses, true, timeout: CommandTimeout).ConfigureAwait(false);
                        Parallel.ForEach(bulkAddresses, address => address.AddressStatuses.ForEach(s => s.AddressId = address.AddressId));

                        if (bulkAddressStatuses.Any()) await _dataRepository.BulkInsertAsync(bulkAddressStatuses, timeout: CommandTimeout).ConfigureAwait(false);
                        _dataRepository.CommitTransaction();
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    _dataRepository.RollbackTransaction();
                }
            }).ConfigureAwait(false);

            if (exceptions.Any())
            {
                if (exceptions.Count == 1) throw exceptions.First();
                throw new AggregateException("One or more errors occured",exceptions);
            }

            return bulkOrganisations.Count;
        }
    }
    #endregion
}
