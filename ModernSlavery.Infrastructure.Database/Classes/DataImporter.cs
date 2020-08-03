using System;
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

        public DataImporter(SharedOptions sharedOptions, IDataRepository dataRepository, IFileRepository fileRepository)
        {
            _sharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _fileRepository = fileRepository ?? throw new ArgumentNullException(nameof(fileRepository));
        }

        /// <summary>
        ///     //Seed the SIC Codes and Categories
        /// </summary>
        /// <param name="context">The database context to initialise</param>
        public async Task ImportSICSectionsAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.SicSections);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = _dataRepository.GetAll<SicSection>();
            if (!force && databaseRecords.Any()) return;

            var fileRecords = await _fileRepository.ReadCSVAsync<SicSection>(filepath, false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            var created = VirtualDateTime.Now;
            foreach (var fileRecord in fileRecords)
            {
                var oldRecord = _dataRepository.Get<SicSection>(fileRecord.SicSectionId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = fileRecord;
                    oldRecord.Created = created;
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

            await _dataRepository.SaveChangesAsync();
        }

        public async Task ImportSICCodesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.SicCodes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = await _dataRepository.GetAll<SicCode>().ToListAsync();
            if (!force && databaseRecords.Any()) return;

            var fileRecords = await _fileRepository.ReadCSVAsync<SicCode>(filepath, false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            var created = VirtualDateTime.Now;
            foreach (var fileRecord in fileRecords)
            {
                var oldRecord = _dataRepository.Get<SicCode>(fileRecord.SicCodeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = fileRecord;
                    oldRecord.Created = created;
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

            await _dataRepository.SaveChangesAsync();
        }

        public async Task ImportStatementDiligenceTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementDiligenceTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = _dataRepository.GetAll<StatementDiligenceType>();
            if (!force && databaseRecords.Any()) return;

            var fileRecords = await _fileRepository.ReadCSVAsync<StatementDiligenceType>(filepath, false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            fileRecords = fileRecords.OrderBy(r => r.ParentDiligenceTypeId).ToList();

            //Add or update the new records
            var created = VirtualDateTime.Now;
            foreach (var fileRecord in fileRecords)
            {
                var oldRecord = _dataRepository.Get<StatementDiligenceType>(fileRecord.StatementDiligenceTypeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = fileRecord;
                    oldRecord.Created = created;
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

            await _dataRepository.SaveChangesAsync();
        }

        public async Task ImportStatementPolicyTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementPolicyTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = _dataRepository.GetAll<StatementPolicyType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementPolicyType>(filepath, false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            var created = VirtualDateTime.Now;
            foreach (var newRecord in newRecords)
            {
                var oldRecord = _dataRepository.Get<StatementPolicyType>(newRecord.StatementPolicyTypeId);
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
            var deletedRecords = _dataRepository.GetAll<StatementPolicyType>().ToList().Where(s => !s.StatementPolicies.Any() && !newRecords.Any(n => n.StatementPolicyTypeId == s.StatementPolicyTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync();
        }

        public async Task ImportStatementRiskTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementRiskTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = _dataRepository.GetAll<StatementRiskType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementRiskType>(filepath, false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");
            newRecords = newRecords.OrderBy(r => r.ParentRiskTypeId).ToList();

            //Add or update the new records
            var created = VirtualDateTime.Now;
            foreach (var newRecord in newRecords)
            {
                var oldRecord = _dataRepository.Get<StatementRiskType>(newRecord.StatementRiskTypeId);
                bool isnew = false;
                if (oldRecord == null)
                {
                    oldRecord = newRecord;
                    oldRecord.Created = created;
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

            await _dataRepository.SaveChangesAsync();
        }

        public async Task ImportStatementSectorTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementSectorTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = _dataRepository.GetAll<StatementSectorType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementSectorType>(filepath, false);
            if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

            //Add or update the new records
            var created = VirtualDateTime.Now;
            foreach (var newRecord in newRecords)
            {
                var oldRecord = _dataRepository.Get<StatementSectorType>(newRecord.StatementSectorTypeId);
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
            var deletedRecords = _dataRepository.GetAll<StatementSectorType>().ToList().Where(s => !s.StatementSectors.Any() && !newRecords.Any(n => n.StatementSectorTypeId == s.StatementSectorTypeId)).ToList();
            foreach (var deletedRecord in deletedRecords)
                _dataRepository.Delete(deletedRecord);

            await _dataRepository.SaveChangesAsync();
        }

        public async Task ImportStatementTrainingTypesAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.StatementTrainingTypes);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = _dataRepository.GetAll<StatementTrainingType>();
            if (!force && databaseRecords.Any()) return;

            var newRecords = await _fileRepository.ReadCSVAsync<StatementTrainingType>(filepath, false);
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

            await _dataRepository.SaveChangesAsync();
        }

        /// <summary>
        ///     //Seed the organisations in the database
        /// </summary>
        /// <param name="context">The database context to initialise</param>
        public async Task ImportOrganisationsAsync(bool force = false)
        {
            var filepath = Path.Combine(_sharedOptions.DataPath, Filenames.ImportOrganisations);

            if (!await _fileRepository.GetDirectoryExistsAsync(_sharedOptions.DataPath)) await _fileRepository.CreateDirectoryAsync(_sharedOptions.DataPath);

            var databaseRecords = _dataRepository.GetAll<Organisation>();
            if (!force && databaseRecords.Any()) return;

            var fileRecords = await _fileRepository.ReadCSVAsync<ImportOrganisationModel>(filepath, false);
            if (!fileRecords.Any()) throw new Exception($"No records found in {filepath}");

            //TODO
            var created = VirtualDateTime.Now;

            await _dataRepository.SaveChangesAsync();
        }
    }
}