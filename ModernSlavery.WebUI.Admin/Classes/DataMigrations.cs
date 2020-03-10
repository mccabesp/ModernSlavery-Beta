using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ModernSlavery.Core.Classes;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using Microsoft.EntityFrameworkCore;
using ModernSlavery.Entities;
using ModernSlavery.SharedKernel;

namespace ModernSlavery.WebUI.Admin
{
    public static class DataMigrations
    {

        /// <summary>
        ///     //Seed the SIC Codes and Categories
        /// </summary>
        /// <param name="context">The database context to initialise</param>
        public static async Task Update_SICSectionsAsync(IDataRepository dataRepository,
            IFileRepository repository,
            string dataPath,
            bool force = false)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(dataPath))
            {
                throw new ArgumentNullException(nameof(dataPath));
            }

            string sectionsPath = Path.Combine(dataPath, Filenames.SicSections);

            if (!await repository.GetDirectoryExistsAsync(dataPath))
            {
                await repository.CreateDirectoryAsync(dataPath);
            }

            var sicSections = dataRepository.GetAll<SicSection>();
            if (force || !sicSections.Any())
            {
                List<SicSection> sectionRecords = await repository.ReadCSVAsync<SicSection>(sectionsPath);
                if (!sectionRecords.Any())
                {
                    throw new Exception($"No records found in {sectionsPath}");
                }

                //sicSections.UpsertRange(sectionRecords);
            }

            await dataRepository.SaveChangesAsync();
        }

        public static async Task Update_SICCodesAsync(IDataRepository dataRepository,
            IFileRepository repository,
            string dataPath,
            bool force = false)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(dataPath))
            {
                throw new ArgumentNullException(nameof(dataPath));
            }

            string codesPath = Path.Combine(dataPath, Filenames.SicCodes);

            if (!await repository.GetDirectoryExistsAsync(dataPath))
            {
                await repository.CreateDirectoryAsync(dataPath);
            }

            List<SicCode> sicCodes = await dataRepository.GetAll<SicCode>().ToListAsync();
            DateTime created = VirtualDateTime.Now;
            if (force || !sicCodes.Any())
            {
                List<SicCode> codeRecords = await repository.ReadCSVAsync<SicCode>(codesPath);
                if (!codeRecords.Any())
                {
                    throw new Exception($"No records found in {codesPath}");
                }

                //This is required to prevent a primary key violation
                Parallel.ForEach(
                    codeRecords,
                    code => {
                        code.SicSection = null;
                        if (sicCodes.Any(
                            s => s.SicCodeId == code.SicCodeId && s.SicSectionId == code.SicSectionId && s.Description == code.Description))
                        {
                            return;
                        }

                        code.Created = created;
                    });
                //dset.UpsertRange(codeRecords);
            }

            await dataRepository.SaveChangesAsync();
        }

    }
}
