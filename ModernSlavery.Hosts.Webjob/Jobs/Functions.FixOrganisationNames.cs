using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Models.LogModels;
using ModernSlavery.Core.SharedKernel;
using Newtonsoft.Json;

namespace ModernSlavery.Hosts.Webjob.Jobs
{
    public partial class Functions
    {
        private async Task FixOrganisationsNamesAsync(ILogger log, string userEmail, string comment)
        {
            if (RunningJobs.Contains(nameof(FixOrganisationsNamesAsync))) return;

            RunningJobs.Add(nameof(FixOrganisationsNamesAsync));
            try
            {
                var orgs = await _SharedBusinessLogic.DataRepository.GetAll<Organisation>().ToListAsync();

                var count = 0;
                var i = 0;
                foreach (var org in orgs)
                {
                    i++;
                    var names = org.OrganisationNames.OrderBy(n => n.Created).ToList();

                    var changed = false;
                    ;
                    while (names.Count > 1 && names[1].Name
                        .EqualsI(names[0].Name.Replace(" LTD.", " LIMITED").Replace(" Ltd", " Limited")))
                    {
                        await _ManualChangeLog.WriteAsync(
                            new ManualChangeLogModel(
                                nameof(FixOrganisationsNamesAsync),
                                ManualActions.Delete,
                                userEmail,
                                nameof(Organisation.EmployerReference),
                                org.EmployerReference,
                                null,
                                JsonConvert.SerializeObject(
                                    new {names[0].Name, names[0].Source, names[0].Created, names[0].OrganisationId}),
                                null,
                                comment));

                        names[1].Created = names[0].Created;
                        _SharedBusinessLogic.DataRepository.Delete(names[0]);
                        names.RemoveAt(0);
                        changed = true;
                    }

                    ;
                    if (names.Count > 0)
                    {
                        var newValue = names[names.Count - 1].Name;
                        if (org.OrganisationName != newValue)
                        {
                            org.OrganisationName = newValue;
                            await _ManualChangeLog.WriteAsync(
                                new ManualChangeLogModel(
                                    nameof(FixOrganisationsNamesAsync),
                                    ManualActions.Update,
                                    userEmail,
                                    nameof(Organisation.EmployerReference),
                                    org.EmployerReference,
                                    nameof(org.OrganisationName),
                                    org.OrganisationName,
                                    newValue,
                                    comment));
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        count++;
                        await _SharedBusinessLogic.DataRepository.SaveChangesAsync();
                    }
                }

                log.LogDebug($"Executed {nameof(FixOrganisationsNamesAsync)} successfully and deleted {count} names");
            }
            finally
            {
                RunningJobs.Remove(nameof(FixOrganisationsNamesAsync));
            }
        }
    }
}