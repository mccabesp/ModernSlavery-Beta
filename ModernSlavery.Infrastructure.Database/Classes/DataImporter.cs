using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModernSlavery.Core;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Core.Models;

namespace ModernSlavery.Infrastructure.Database.Classes
{
    public class DataImporter : IDataImporter
    {
        private readonly SharedOptions _sharedOptions;
        private readonly IDataRepository _dataRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IReportingDeadlineHelper _reportingDeadlineHelper;
        private readonly IPostcodeChecker _postcodeChecker;

        public DataImporter(SharedOptions sharedOptions, IDataRepository dataRepository, IFileRepository fileRepository, IReportingDeadlineHelper reportingDeadlineHelper, IPostcodeChecker postcodeChecker)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
            _reportingDeadlineHelper = reportingDeadlineHelper ?? throw new ArgumentNullException(nameof(reportingDeadlineHelper));
            _postcodeChecker = postcodeChecker ?? throw new ArgumentNullException(nameof(postcodeChecker));

        }

        #region Seed the SIC Codes and Categories
        public async Task ImportSICSectionsAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.SicSections);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<SicSection>();
            if (!force && databaseRecords.Any()) return;

            var fileRecords = await _fileRepository.ReadCSVAsync<SicSection>(filepath, false).ConfigureAwait(false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
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
                    _dataRepository.Insert(oldRecord);
                else
                    _dataRepository.Update(oldRecord);
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<SicSection>().ToList().Where(s => !s.SicCodes.Any() && !fileRecords.Any(n => n.SicSectionId == s.SicSectionId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task ImportSICCodesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.SicCodes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            var databaseRecords = await _dataRepository.GetAll<SicCode>().ToListAsync().ConfigureAwait(false);
            if (!force && databaseRecords.Any()) return;

            var fileRecords = await _fileRepository.ReadCSVAsync<SicCode>(filepath, false).ConfigureAwait(false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
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
                    _dataRepository.Insert(oldRecord);
                else
                    _dataRepository.Update(oldRecord);
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<SicCode>().ToList().Where(s => !s.OrganisationSicCodes.Any() && !fileRecords.Any(n => n.SicCodeId == s.SicCodeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
        }
        #endregion

        #region Seed Statement types
        public async Task ImportStatementDiligenceTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementDiligenceTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<StatementDiligenceType>();
            if (!force && databaseRecords.Any()) return;

            var fileRecords = await _fileRepository.ReadCSVAsync<StatementDiligenceType>(filepath, false).ConfigureAwait(false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            fileRecords = fileRecords.OrderBy(r => r.ParentDiligenceTypeId).ToList();

            //Add or update the new records
            foreach (var fileRecord in fileRecords)
            {
                var oldRecord = _dataRepository.Get<StatementDiligenceType>(fileRecord.StatementDiligenceTypeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = fileRecord;
                    isnew = true;
                }

                if (fileRecord.ParentDiligenceTypeId != null) oldRecord.ParentDiligenceType = _dataRepository.Get<StatementDiligenceType>(fileRecord.ParentDiligenceTypeId);
                oldRecord.ParentDiligenceTypeId = fileRecord.ParentDiligenceTypeId;
                oldRecord.Description = fileRecord.Description;
                if (isnew)
                    _dataRepository.Insert(oldRecord);
                else
                    _dataRepository.Update(oldRecord);
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<StatementDiligenceType>().ToList().Where(s => !s.StatementDiligences.Any() && !fileRecords.Any(n => n.StatementDiligenceTypeId == s.StatementDiligenceTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task ImportStatementPolicyTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementPolicyTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<StatementPolicyType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementPolicyType>(filepath, false).ConfigureAwait(false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            foreach (var newRecord in newRecords)
            {
                var oldRecord = _dataRepository.Get<StatementPolicyType>(newRecord.StatementPolicyTypeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = newRecord;
                    isnew = true;
                }

                oldRecord.Description = newRecord.Description;
                if (isnew)
                    _dataRepository.Insert(oldRecord);
                else
                    _dataRepository.Update(oldRecord);
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<StatementPolicyType>().ToList().Where(s => !s.StatementPolicies.Any() && !newRecords.Any(n => n.StatementPolicyTypeId == s.StatementPolicyTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task ImportStatementRiskTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementRiskTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<StatementRiskType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementRiskType>(filepath, false).ConfigureAwait(false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");
            newRecords = newRecords.OrderBy(r => r.ParentRiskTypeId).ToList();

            //Add or update the new records
            foreach (var newRecord in newRecords)
            {
                var oldRecord = _dataRepository.Get<StatementRiskType>(newRecord.StatementRiskTypeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = newRecord;
                    isnew = true;
                }

                if (newRecord.ParentRiskTypeId != null) oldRecord.ParentRiskType = _dataRepository.Get<StatementRiskType>(newRecord.ParentRiskTypeId);
                oldRecord.ParentRiskTypeId = newRecord.ParentRiskTypeId;
                oldRecord.Description = newRecord.Description;
                oldRecord.Category = newRecord.Category;

                if (isnew)
                    _dataRepository.Insert(oldRecord);
                else
                    _dataRepository.Update(oldRecord);
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<StatementRiskType>().ToList().Where(s => !s.StatementRelevantRisks.Any() && !s.StatementHighRisks.Any() && !s.StatementLocationRisks.Any() && !newRecords.Any(n => n.StatementRiskTypeId == s.StatementRiskTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task ImportStatementSectorTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementSectorTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<StatementSectorType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementSectorType>(filepath, false).ConfigureAwait(false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
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
                    _dataRepository.Insert(oldRecord);
                else
                    _dataRepository.Update(oldRecord);
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<StatementSectorType>().ToList().Where(s => !s.StatementSectors.Any() && !newRecords.Any(n => n.StatementSectorTypeId == s.StatementSectorTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task ImportStatementTrainingTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementTrainingTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            var databaseRecords = _dataRepository.GetAll<StatementTrainingType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementTrainingType>(filepath, false).ConfigureAwait(false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            var created = VirtualDateTime.Now;
            foreach (var newRecord in newRecords)
            {
                var oldRecord = _dataRepository.Get<StatementTrainingType>(newRecord.StatementTrainingTypeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = newRecord;
                    oldRecord.Created = created;
                    isnew = true;
                }

                oldRecord.Description = newRecord.Description;
                if (isnew)
                    _dataRepository.Insert(oldRecord);
                else
                    _dataRepository.Update(oldRecord);
            }

            //Delete the old records
            var deletedRecords = _dataRepository.GetAll<StatementTrainingType>().ToList().Where(s => !s.StatementTraining.Any() && !newRecords.Any(n => n.StatementTrainingTypeId == s.StatementTrainingTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
        }
        #endregion

        #region Seed Organisations
        /// <summary>
        ///     //Seed the private organisations in the database
        /// </summary>
        /// <param name="force">When true always imports. When false only when no private organisation records in db</param>
        public async Task ImportPrivateOrganisationsAsync(long userId, bool force = false)
        {
            await ImportOrganisationsAsync(Filenames.ImportPublicOrganisations, SectorTypes.Private, userId, force).ConfigureAwait(false);
        }

        /// <summary>
        ///     //Seed the public organisations in the database
        /// </summary>
        /// <param name="force">When true always imports. When false only when no public organisation records in db</param>
        public async Task ImportPublicOrganisationsAsync(long userId, bool force = false)
        {
            await ImportOrganisationsAsync(Filenames.ImportPublicOrganisations, SectorTypes.Public, userId, force).ConfigureAwait(false);
        }

        private async Task ImportOrganisationsAsync(string fileName, SectorTypes sectorType, long userId, bool force = false)
        {
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));
            if (sectorType == SectorTypes.Unknown) throw new ArgumentOutOfRangeException(nameof(sectorType));
            if (userId == 0) throw new ArgumentOutOfRangeException(nameof(userId), "UserId cannot be 0");

            var filepath = Path.Combine(_sharedOptions.DataPath, fileName);

            //Create the directory if it doesnt exist
            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath).ConfigureAwait(false)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath).ConfigureAwait(false);

            //Dont import unless forced when there are existing records
            if (!force && await _dataRepository.CountAsync<Organisation>(r => r.SectorType == sectorType).ConfigureAwait(false) > 0) return;

            //Get the database records
            var databaseRecords = _dataRepository.ToListAsync<Organisation>(r => r.SectorType == sectorType);

            //Get the imported records
            var newRecords = await _fileRepository.ReadCSVAsync<ImportOrganisationModel>(filepath, false).ConfigureAwait(false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            var orgNames = new SortedSet<string>();
            var companyNumbers = new SortedSet<string>();

            var allSicCodes = _dataRepository.GetAll<SicCode>().Select(s => s.SicCodeId).ToSortedSet();

            var exceptions = new List<Exception>();

            //Add or update the new records
            var created = VirtualDateTime.Now;
            int index = -1;
            foreach (var newRecord in newRecords)
            {
                index++;
                var recordExceptions = new List<Exception>();

                try
                {
                    //Check organisation name in the file
                    if (!string.IsNullOrWhiteSpace(newRecord.OrganisationName))
                        recordExceptions.Add(new Exception($"Empty organisation name '{fileName}'"));

                    //Check the sector is as expected in the file
                    if (!sectorType.ToString().EqualsI(newRecord.Sector))
                        recordExceptions.Add(new Exception($"Invalid sector '{newRecord.Sector}' in {sectorType.ToString().ToLower()} file '{fileName}'"));

                    //Check the sic codes
                    if (string.IsNullOrWhiteSpace(newRecord.SICCode))
                        recordExceptions.Add(new Exception($"Missing SicCode for organisation '{newRecord.OrganisationName}'"));

                    //Check post code
                    if (string.IsNullOrWhiteSpace(newRecord.PostCode))
                        recordExceptions.Add(new Exception($"Missing PostCode for organisation '{newRecord.OrganisationName}'"));
                    else if (newRecord.PostCode.Length > 20)
                        recordExceptions.Add(new Exception($"{nameof(newRecord.PostCode)} exceeds 20 characters for organisation '{newRecord.OrganisationName}'"));

                    //Check address field length
                    if (!string.IsNullOrWhiteSpace(newRecord.Address1) && newRecord.Address1.Length > 100)
                        recordExceptions.Add(new Exception($"{nameof(newRecord.Address1)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'"));
                    if (!string.IsNullOrWhiteSpace(newRecord.Address2) && newRecord.Address2.Length > 100)
                        recordExceptions.Add(new Exception($"{nameof(newRecord.Address2)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'"));
                    if (!string.IsNullOrWhiteSpace(newRecord.Address3) && newRecord.Address3.Length > 100)
                        recordExceptions.Add(new Exception($"{nameof(newRecord.Address3)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'"));
                    if (!string.IsNullOrWhiteSpace(newRecord.TownCity) && newRecord.TownCity.Length > 100)
                        recordExceptions.Add(new Exception($"{nameof(newRecord.TownCity)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'"));
                    if (!string.IsNullOrWhiteSpace(newRecord.County) && newRecord.County.Length > 100)
                        recordExceptions.Add(new Exception($"{nameof(newRecord.County)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'"));
                    if (!string.IsNullOrWhiteSpace(newRecord.Country) && newRecord.Country.Length > 100)
                        recordExceptions.Add(new Exception($"{nameof(newRecord.Country)} exceeds 100 characters for organisation '{newRecord.OrganisationName}'"));

                    var newSicCodes = newRecord.SICCode.SplitI(";,: ").Select(c => c.ToInt32());
                    var badSicCodes = newSicCodes.Except(allSicCodes);
                    if (badSicCodes.Any())
                        recordExceptions.Add(new Exception($"Invalid SicCodes '{badSicCodes.ToDelimitedString()}' for organisation '{newRecord.OrganisationName}'"));

                    //Try and get the existing organisation
                    Organisation org = null;
                    if (!string.IsNullOrWhiteSpace(newRecord.CompanyNumber))
                        org = await _dataRepository.SingleOrDefaultAsync<Organisation>(o => o.CompanyNumber == newRecord.CompanyNumber).ConfigureAwait(false);
                    else if (sectorType == SectorTypes.Public)
                        org = await _dataRepository.SingleOrDefaultAsync<Organisation>(o => string.Equals(newRecord.OrganisationName, o.OrganisationName, StringComparison.OrdinalIgnoreCase)).ConfigureAwait(false);
                    else
                        recordExceptions.Add(new Exception($"Attempt to private import organisation '{newRecord.CompanyNumber}' with missing CompanyNumber"));

                    if (org != null) continue;

                    if (recordExceptions.Any())
                    {
                        exceptions.AddRange(recordExceptions);
                        continue;
                    }

                    org = new Organisation()
                    {
                        SectorType = sectorType,
                        OrganisationName = newRecord.OrganisationName,
                        CompanyNumber = newRecord.CompanyNumber.IsNumber() ? newRecord.CompanyNumber.PadLeft(8, '0') : newRecord.CompanyNumber,
                    };

                    org.SetStatus(OrganisationStatuses.Active, userId);

                    //Add the name
                    var orgName = new OrganisationName { Name = org.OrganisationName, Source = "External" };
                    _dataRepository.Insert(orgName);
                    org.OrganisationNames.Add(orgName);

                    //Add the new address
                    var address = new OrganisationAddress();
                    address.Organisation = org;
                    address.CreatedByUserId = userId;
                    address.Address1 = newRecord.Address1;
                    address.Address2 = newRecord.Address2;
                    address.Address3 = newRecord.Address3;
                    address.TownCity = newRecord.TownCity;
                    address.County = newRecord.County;
                    address.Country = newRecord.Country;
                    address.PostCode = newRecord.PostCode;
                    address.Source = "External";
                    address.SetStatus(AddressStatuses.Active, userId);
                    _dataRepository.Insert(address);

                    //Add the new sicCode
                    foreach (var sicCodeId in newSicCodes)
                    {
                        var sicCode = new OrganisationSicCode { Organisation = org, SicCodeId = sicCodeId, Source = "External" };
                        _dataRepository.Insert(sicCode);
                        org.OrganisationSicCodes.Add(sicCode);
                    }

                    _dataRepository.Insert(org);

                    await _dataRepository.SaveChangesAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    recordExceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                if (exceptions.Count == 1) throw exceptions[0];
                throw new AggregateException(exceptions);
            }
        }
        #endregion
    }
}