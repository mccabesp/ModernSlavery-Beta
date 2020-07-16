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

namespace ModernSlavery.WebUI.Admin.Classes
{
    public static class DataMigrations
    {
        /// <summary>
        ///     //Seed the SIC Codes and Categories
        /// </summary>
        /// <param name="context">The database context to initialise</param>
        public static async Task Update_SICSectionsAsync(IDataRepository dataRepository,
            IFileRepository fileRepository,
            string dataPath,
            bool force = false)
        {
            if (fileRepository == null) throw new ArgumentNullException(nameof(fileRepository));

            if (string.IsNullOrWhiteSpace(dataPath)) throw new ArgumentNullException(nameof(dataPath));

            var filepath = Path.Combine(dataPath, Filenames.SicSections);

            if (!await fileRepository.GetDirectoryExistsAsync(dataPath)) await fileRepository.CreateDirectoryAsync(dataPath);

            var sicSections = dataRepository.GetAll<SicSection>();
            if (force || !sicSections.Any())
            {
                var sectionRecords = await fileRepository.ReadCSVAsync<SicSection>(filepath);
                if (!sectionRecords.Any()) throw new Exception($"No records found in {filepath}");

                //sicSections.UpsertRange(sectionRecords);
            }

            await dataRepository.SaveChangesAsync();
        }

        public static async Task Update_SICCodesAsync(IDataRepository dataRepository,
            IFileRepository fileRepository,
            string dataPath,
            bool force = false)
        {
            if (fileRepository == null) throw new ArgumentNullException(nameof(fileRepository));

            if (string.IsNullOrWhiteSpace(dataPath)) throw new ArgumentNullException(nameof(dataPath));

            var filepath = Path.Combine(dataPath, Filenames.SicCodes);

            if (!await fileRepository.GetDirectoryExistsAsync(dataPath)) await fileRepository.CreateDirectoryAsync(dataPath);

            var sicCodes = await dataRepository.GetAll<SicCode>().ToListAsync();
            var created = VirtualDateTime.Now;
            if (force || !sicCodes.Any())
            {
                var codeRecords = await fileRepository.ReadCSVAsync<SicCode>(filepath);
                if (!codeRecords.Any()) throw new Exception($"No records found in {filepath}");

                //This is required to prevent a primary key violation
                Parallel.ForEach(
                    codeRecords,
                    code =>
                    {
                        code.SicSection = null;
                        if (sicCodes.Any(
                            s => s.SicCodeId == code.SicCodeId && s.SicSectionId == code.SicSectionId &&
                                 s.Description == code.Description))
                            return;

                        code.Created = created;
                    });
                //dset.UpsertRange(codeRecords);
            }

            await dataRepository.SaveChangesAsync();
        }

        public static async Task Update_StatementDiligenceTypesAsync(IDataRepository dataRepository,
           IFileRepository fileRepository,
           string dataPath,
           bool force = false)
        {
            if (fileRepository == null) throw new ArgumentNullException(nameof(fileRepository));

            if (string.IsNullOrWhiteSpace(dataPath)) throw new ArgumentNullException(nameof(dataPath));

            var filepath = Path.Combine(dataPath, Filenames.StatementDiligenceTypes);

            if (!await fileRepository.GetDirectoryExistsAsync(dataPath)) await fileRepository.CreateDirectoryAsync(dataPath);

            var empty = !await dataRepository.GetAll<StatementDiligenceType>().AnyAsync();
            var created = VirtualDateTime.Now;
            if (force || empty)
            {
                var newRecords = await fileRepository.ReadCSVAsync<StatementDiligenceType>(filepath);
                if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");
                newRecords = newRecords.OrderBy(r => r.ParentDiligenceTypeId).ToList();

                //Add or update the new records
                foreach( var newRecord in newRecords)
                {
                    var oldRecord = dataRepository.Get<StatementDiligenceType>(newRecord.StatementDiligenceTypeId);
                    bool isnew= false;
                    if (oldRecord == null)
                    {
                        oldRecord = newRecord;
                        oldRecord.Created = created;
                        isnew = true;
                    }

                    if (newRecord.ParentDiligenceTypeId!=null)oldRecord.ParentDiligenceType = dataRepository.Get<StatementDiligenceType>(newRecord.ParentDiligenceTypeId);
                    oldRecord.ParentDiligenceTypeId = newRecord.ParentDiligenceTypeId;
                    oldRecord.Description = newRecord.Description;
                    if (isnew)
                        dataRepository.Insert(oldRecord);
                    else
                        dataRepository.Update(oldRecord);
                }

                //Delete the old records
                var deletedRecords = dataRepository.GetAll<StatementDiligenceType>().ToList().Where(s => !s.StatementDiligences.Any() && !newRecords.Any(n => n.StatementDiligenceTypeId == s.StatementDiligenceTypeId)).ToList();
                foreach (var deletedRecord in deletedRecords)
                    dataRepository.Delete(deletedRecord);
            }

            await dataRepository.SaveChangesAsync();
        }

        public static async Task Update_StatementPolicyTypesAsync(IDataRepository dataRepository,
          IFileRepository fileRepository,
          string dataPath,
          bool force = false)
        {
            if (fileRepository == null) throw new ArgumentNullException(nameof(fileRepository));

            if (string.IsNullOrWhiteSpace(dataPath)) throw new ArgumentNullException(nameof(dataPath));

            var filepath = Path.Combine(dataPath, Filenames.StatementPolicyTypes);

            if (!await fileRepository.GetDirectoryExistsAsync(dataPath)) await fileRepository.CreateDirectoryAsync(dataPath);

            var empty = !await dataRepository.GetAll<StatementPolicyType>().AnyAsync();
            var created = VirtualDateTime.Now;
            if (force || empty)
            {
                var newRecords = await fileRepository.ReadCSVAsync<StatementPolicyType>(filepath);
                if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

                //Add or update the new records
                foreach (var newRecord in newRecords)
                {
                    var oldRecord = dataRepository.Get<StatementPolicyType>(newRecord.StatementPolicyTypeId);
                    bool isnew = false;
                    if (oldRecord == null)
                    {
                        oldRecord = newRecord;
                        oldRecord.Created = created;
                        isnew = true;
                    }

                    oldRecord.Description = newRecord.Description;
                    if (isnew)
                        dataRepository.Insert(oldRecord);
                    else
                        dataRepository.Update(oldRecord);
                }

                //Delete the old records
                var deletedRecords = dataRepository.GetAll<StatementPolicyType>().ToList().Where(s => !s.StatementPolicies.Any() && !newRecords.Any(n => n.StatementPolicyTypeId == s.StatementPolicyTypeId)).ToList();
                foreach (var deletedRecord in deletedRecords)
                    dataRepository.Delete(deletedRecord);
            }

            await dataRepository.SaveChangesAsync();
        }

        public static async Task Update_StatementRiskTypesAsync(IDataRepository dataRepository,
          IFileRepository fileRepository,
          string dataPath,
          bool force = false)
        {
            if (fileRepository == null) throw new ArgumentNullException(nameof(fileRepository));

            if (string.IsNullOrWhiteSpace(dataPath)) throw new ArgumentNullException(nameof(dataPath));

            var filepath = Path.Combine(dataPath, Filenames.StatementRiskTypes);

            if (!await fileRepository.GetDirectoryExistsAsync(dataPath)) await fileRepository.CreateDirectoryAsync(dataPath);

            var empty = !await dataRepository.GetAll<StatementRiskType>().AnyAsync();
            var created = VirtualDateTime.Now;
            if (force || empty)
            {
                var newRecords = await fileRepository.ReadCSVAsync<StatementRiskType>(filepath);
                if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");
                newRecords = newRecords.OrderBy(r => r.ParentRiskTypeId).ToList();

                //Add or update the new records
                foreach (var newRecord in newRecords)
                {
                    var oldRecord = dataRepository.Get<StatementRiskType>(newRecord.StatementRiskTypeId);
                    bool isnew = false;
                    if (oldRecord == null)
                    {
                        oldRecord = newRecord;
                        oldRecord.Created = created;
                        isnew = true;
                    }

                    if (newRecord.ParentRiskTypeId != null) oldRecord.ParentRiskType = dataRepository.Get<StatementRiskType>(newRecord.ParentRiskTypeId);
                    oldRecord.ParentRiskTypeId = newRecord.ParentRiskTypeId;
                    oldRecord.Description = newRecord.Description;
                    oldRecord.Category = newRecord.Category;

                    if (isnew) 
                        dataRepository.Insert(oldRecord);
                    else
                        dataRepository.Update(oldRecord);
                }

                //Delete the old records
                var deletedRecords = dataRepository.GetAll<StatementRiskType>().ToList().Where(s => !s.StatementRelevantRisks.Any() && !s.StatementHighRisks.Any() && !s.StatementLocationRisks.Any() && !newRecords.Any(n => n.StatementRiskTypeId == s.StatementRiskTypeId)).ToList();
                foreach (var deletedRecord in deletedRecords)
                    dataRepository.Delete(deletedRecord);
            }

            await dataRepository.SaveChangesAsync();
        }

        public static async Task Update_StatementSectorTypesAsync(IDataRepository dataRepository,
          IFileRepository fileRepository,
          string dataPath,
          bool force = false)
        {
            if (fileRepository == null) throw new ArgumentNullException(nameof(fileRepository));

            if (string.IsNullOrWhiteSpace(dataPath)) throw new ArgumentNullException(nameof(dataPath));

            var filepath = Path.Combine(dataPath, Filenames.StatementSectorTypes);

            if (!await fileRepository.GetDirectoryExistsAsync(dataPath)) await fileRepository.CreateDirectoryAsync(dataPath);

            var empty = !await dataRepository.GetAll<StatementSectorType>().AnyAsync();
            var created = VirtualDateTime.Now;
            if (force || empty)
            {
                var newRecords = await fileRepository.ReadCSVAsync<StatementSectorType>(filepath);
                if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

                //Add or update the new records
                foreach (var newRecord in newRecords)
                {
                    var oldRecord = dataRepository.Get<StatementSectorType>(newRecord.StatementSectorTypeId);
                    bool isnew = false;
                    if (oldRecord == null)
                    {
                        oldRecord = newRecord;
                        oldRecord.Created = created;
                        isnew = true;
                    }

                    oldRecord.Description = newRecord.Description;
                    if (isnew)
                        dataRepository.Insert(oldRecord);
                    else
                        dataRepository.Update(oldRecord);
                }

                //Delete the old records
                var deletedRecords = dataRepository.GetAll<StatementSectorType>().ToList().Where(s => !s.StatementSectors.Any() && !newRecords.Any(n => n.StatementSectorTypeId == s.StatementSectorTypeId)).ToList();
                foreach (var deletedRecord in deletedRecords)
                    dataRepository.Delete(deletedRecord);
            }

            await dataRepository.SaveChangesAsync();
        }

        public static async Task Update_StatementTrainingTypesAsync(IDataRepository dataRepository,
          IFileRepository fileRepository,
          string dataPath,
          bool force = false)
        {
            if (fileRepository == null) throw new ArgumentNullException(nameof(fileRepository));

            if (string.IsNullOrWhiteSpace(dataPath)) throw new ArgumentNullException(nameof(dataPath));

            var filepath = Path.Combine(dataPath, Filenames.StatementTrainingTypes);

            if (!await fileRepository.GetDirectoryExistsAsync(dataPath)) await fileRepository.CreateDirectoryAsync(dataPath);

            var empty = !await dataRepository.GetAll<StatementTrainingType>().AnyAsync();
            var created = VirtualDateTime.Now;
            if (force || empty)
            {
                var newRecords = await fileRepository.ReadCSVAsync<StatementTrainingType>(filepath);
                if (!newRecords.Any()) throw new Exception($"No records found in {filepath}");

                //Add or update the new records
                foreach (var newRecord in newRecords)
                {
                    var oldRecord = dataRepository.Get<StatementTrainingType>(newRecord.StatementTrainingTypeId);
                    bool isnew = false;
                    if (oldRecord == null)
                    {
                        oldRecord = newRecord;
                        oldRecord.Created = created;
                        isnew = true;
                    }

                    oldRecord.Description = newRecord.Description;
                    if (isnew)
                        dataRepository.Insert(oldRecord);
                    else
                        dataRepository.Update(oldRecord);
                }

                //Delete the old records
                var deletedRecords = dataRepository.GetAll<StatementTrainingType>().ToList().Where(s => !s.StatementTraining.Any() && !newRecords.Any(n => n.StatementTrainingTypeId == s.StatementTrainingTypeId)).ToList();
                foreach (var deletedRecord in deletedRecords)
                    dataRepository.Delete(deletedRecord);
            }

            await dataRepository.SaveChangesAsync();
        }
    }
}