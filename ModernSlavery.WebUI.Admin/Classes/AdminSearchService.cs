using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ModernSlavery.Core.Entities;
using ModernSlavery.Core.Extensions;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.WebUI.Admin.Models;

namespace ModernSlavery.WebUI.Admin.Classes
{
    internal class AdminSearchServiceOrganisation
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public List<string> OrganisationNames { get; set; } // All names (current and previous)
        public string CompanyNumber { get; set; }
        public string OrganisationReference { get; set; }
    }

    internal class AdminSearchServiceUser
    {
        public long UserId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
    }

    public class AdminSearchService
    {
        private readonly IEventLogger CustomLogger;

        private readonly IDataRepository dataRepository;

        private List<AdminSearchServiceOrganisation> _cachedOrganisations;

        private List<AdminSearchServiceUser> _cachedUsers;
        private DateTime cacheLastUpdated = DateTime.MinValue;

        private readonly object cacheLock = new object();

        public AdminSearchService(IEventLogger customLogger, IDataRepository dataRepository)
        {
            CustomLogger = customLogger;
            this.dataRepository = dataRepository;
            EnsureCacheUpdated();
        }

        internal List<AdminSearchServiceOrganisation> cachedOrganisations
        {
            get
            {
                EnsureCacheUpdated();
                return _cachedOrganisations;
            }
        }

        internal List<AdminSearchServiceUser> cachedUsers
        {
            get
            {
                EnsureCacheUpdated();
                return _cachedUsers;
            }
        }

        private void EnsureCacheUpdated()
        {
            lock (cacheLock)
            {
                if (cacheLastUpdated.AddMinutes(1) > VirtualDateTime.Now) return;

                CustomLogger.Information("Starting cache update (AdminSearchService.StartCacheUpdateThread)");

                _cachedOrganisations = LoadAllOrganisations(dataRepository);
                _cachedUsers = LoadAllUsers(dataRepository);

                cacheLastUpdated = VirtualDateTime.Now;

                CustomLogger.Information("Finished cache update (AdminSearchService.StartCacheUpdateThread)");
            }
        }

        public AdminSearchResultsViewModel Search(string query)
        {
            var searchTerms = ExtractSearchTermsFromQuery(query);

            List<AdminSearchServiceOrganisation> allOrganisations;
            List<AdminSearchServiceUser> allUsers;
            DateTime timeDetailsLoaded;
            bool usedCache;

            var loadingStart = VirtualDateTime.Now;
            if (cacheLastUpdated < VirtualDateTime.Now.AddSeconds(-70))
            {
                allOrganisations = LoadAllOrganisations(dataRepository);
                allUsers = LoadAllUsers(dataRepository);
                timeDetailsLoaded = VirtualDateTime.Now;
                usedCache = false;
            }
            else
            {
                allOrganisations = cachedOrganisations;
                allUsers = cachedUsers;
                timeDetailsLoaded = cacheLastUpdated;
                usedCache = true;
            }

            var loadingEnd = VirtualDateTime.Now;

            var filteringStart = VirtualDateTime.Now;
            var matchingOrganisations = GetMatchingOrganisations(allOrganisations, searchTerms, query);
            var matchingUsers = GetMatchingUsers(allUsers, searchTerms);
            var filteringEnd = VirtualDateTime.Now;

            var orderingStart = VirtualDateTime.Now;
            var matchingOrganisationsOrderedByName =
                matchingOrganisations.OrderBy(o => o.OrganisationName.ToLower()).ToList();
            var matchingUsersOrderedByName =
                matchingUsers.OrderBy(u => u.FullName).ToList();
            var orderingEnd = VirtualDateTime.Now;

            var highlightingStart = VirtualDateTime.Now;
            var matchingOrganisationsWithHighlightedMatches =
                HighlightOrganisationMatches(matchingOrganisationsOrderedByName, searchTerms, query);
            var matchingUsersWithHighlightedMatches =
                HighlightUserMatches(matchingUsersOrderedByName, searchTerms);
            var highlightingEnd = VirtualDateTime.Now;

            var results = new AdminSearchResultsViewModel
            {
                OrganisationResults = matchingOrganisationsWithHighlightedMatches,
                UserResults = matchingUsersWithHighlightedMatches,

                LoadingMilliSeconds = loadingEnd.Subtract(loadingStart).TotalMilliseconds,
                FilteringMilliSeconds = filteringEnd.Subtract(filteringStart).TotalMilliseconds,
                OrderingMilliSeconds = orderingEnd.Subtract(orderingStart).TotalMilliseconds,
                HighlightingMilliSeconds = highlightingEnd.Subtract(highlightingStart).TotalMilliseconds,

                SearchCacheUpdatedSecondsAgo = (int) VirtualDateTime.Now.Subtract(timeDetailsLoaded).TotalSeconds,
                UsedCache = usedCache
            };
            return results;
        }

        private List<string> ExtractSearchTermsFromQuery(string query)
        {
            return query.Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(st => st.ToLower())
                .ToList();
        }

        internal static List<AdminSearchServiceOrganisation> LoadAllOrganisations(IDataRepository repository)
        {
            return repository
                .GetAll<Organisation>()
                .Include(o => o.OrganisationNames)
                .Select(o => new AdminSearchServiceOrganisation
                {
                    OrganisationId = o.OrganisationId,
                    OrganisationName = o.OrganisationName,
                    CompanyNumber = o.CompanyNumber,
                    OrganisationReference = o.OrganisationReference,
                    OrganisationNames = o.OrganisationNames.Select(on => on.Name).ToList()
                })
                .ToList();
        }

        internal static List<AdminSearchServiceUser> LoadAllUsers(IDataRepository repository)
        {
            return repository
                .GetAll<User>()
                .Select(u => new AdminSearchServiceUser
                {
                    UserId = u.UserId,
                    FullName = u.Fullname,
                    EmailAddress = u.EmailAddress
                })
                .ToList();
        }

        private List<AdminSearchServiceOrganisation> GetMatchingOrganisations(
            List<AdminSearchServiceOrganisation> allOrganisations,
            List<string> searchTerms,
            string query)
        {
            return allOrganisations
                .Where(
                    organisation =>
                    {
                        var nameMatches =
                            CurrentOrPreviousOrganisationNameMatchesSearchTerms(organisation, searchTerms);
                        var organisationRefMatches = organisation.OrganisationReference?.Trim() == query.Trim();
                        var companyNumberMatches = organisation.CompanyNumber?.Trim() == query.Trim();
                        return nameMatches || organisationRefMatches || companyNumberMatches;
                    })
                .ToList();
        }

        private List<AdminSearchServiceUser> GetMatchingUsers(List<AdminSearchServiceUser> allUsers,
            List<string> searchTerms)
        {
            return allUsers
                .Where(user =>
                    NameMatchesSearchTerms(user.FullName, searchTerms) ||
                    NameMatchesSearchTerms(user.EmailAddress, searchTerms))
                .ToList();
        }

        private bool CurrentOrPreviousOrganisationNameMatchesSearchTerms(AdminSearchServiceOrganisation organisation,
            List<string> searchTerms)
        {
            return organisation.OrganisationNames.Any(on => NameMatchesSearchTerms(on, searchTerms));
        }

        private bool NameMatchesSearchTerms(string name, List<string> searchTerms)
        {
            return searchTerms.All(st => name.ToLower().Contains(st));
        }

        private List<AdminSearchResultOrganisationViewModel> HighlightOrganisationMatches(
            List<AdminSearchServiceOrganisation> organisations,
            List<string> searchTerms,
            string query)
        {
            return organisations
                .Select(
                    organisation =>
                    {
                        var matchGroupsForCurrentName = GetMatchGroups(organisation.OrganisationName, searchTerms);

                        var previousNames = organisation.OrganisationNames
                            .Except(new[] {organisation.OrganisationName});

                        var matchGroupsForPreviousNames = previousNames
                            .Where(on => NameMatchesSearchTerms(on, searchTerms))
                            .Select(on => GetMatchGroups(on, searchTerms))
                            .ToList();

                        var organisationRefMatch = organisation.OrganisationReference?.Trim() == query.Trim()
                            ? organisation.OrganisationReference
                            : null;

                        var companyNumberMatch = organisation.CompanyNumber?.Trim() == query.Trim()
                            ? organisation.CompanyNumber
                            : null;

                        return new AdminSearchResultOrganisationViewModel
                        {
                            OrganisationName = matchGroupsForCurrentName,
                            OrganisationPreviousNames = matchGroupsForPreviousNames,
                            OrganisationRef = organisationRefMatch,
                            CompanyNumber = companyNumberMatch,
                            OrganisationId = organisation.OrganisationId
                        };
                    })
                .ToList();
        }

        private List<AdminSearchResultUserViewModel> HighlightUserMatches(
            List<AdminSearchServiceUser> users,
            List<string> searchTerms
        )
        {
            return users
                .Select(
                    user =>
                    {
                        var matchGroupsForFullName = GetMatchGroups(user.FullName, searchTerms);
                        var matchGroupsForEmailAddress = GetMatchGroups(user.EmailAddress, searchTerms);

                        return new AdminSearchResultUserViewModel
                        {
                            UserFullName = matchGroupsForFullName, UserEmailAddress = matchGroupsForEmailAddress,
                            UserId = user.UserId
                        };
                    })
                .ToList();
        }

        private AdminSearchMatchViewModel GetMatchGroups(string organisationName, List<string> searchTerms)
        {
            var matchGroups = new List<AdminSearchMatchGroupViewModel>();

            var stillSearching = true;
            var searchStart = 0;
            while (stillSearching)
            {
                var nextMatch = GetNextMatch(organisationName, searchTerms, searchStart);
                if (nextMatch != null)
                {
                    matchGroups.Add(nextMatch);
                    searchStart = nextMatch.Start + nextMatch.Length;
                    if (searchStart >= organisationName.Length) stillSearching = false;
                }
                else
                {
                    stillSearching = false;
                }
            }

            return new AdminSearchMatchViewModel {Text = organisationName, MatchGroups = matchGroups};
        }

        private static AdminSearchMatchGroupViewModel GetNextMatch(string organisationName, List<string> searchTerms,
            int searchStart)
        {
            var possibleMatches = new List<AdminSearchMatchGroupViewModel>();

            foreach (var searchTerm in searchTerms)
            {
                var matchStart = organisationName.IndexOf(searchTerm, searchStart,
                    StringComparison.InvariantCultureIgnoreCase);
                if (matchStart != -1)
                    possibleMatches.Add(new AdminSearchMatchGroupViewModel
                        {Start = matchStart, Length = searchTerm.Length});
            }

            return possibleMatches
                .OrderBy(m => m.Start)
                .ThenByDescending(m => m.Length)
                .FirstOrDefault();
        }
    }
}