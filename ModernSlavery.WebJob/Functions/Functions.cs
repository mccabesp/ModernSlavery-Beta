using System;
using ModernSlavery.BusinessLogic;
using ModernSlavery.BusinessLogic.Services;
using ModernSlavery.Core.Interfaces;
using ModernSlavery.Extensions;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using ModernSlavery.Core.Models;
using ModernSlavery.Infrastructure.Message;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.WebJob
{
    public partial class Functions
    {

        public Functions(
            ICustomLogger customLogger,
            IMessenger messenger,
            IFileRepository fileRepository,
            IDataRepository dataRepository,
            ISearchRepository<EmployerSearchModel> employerSearchRepository,
            ISearchRepository<SicCodeSearchModel> sicCodeSearchRepository,
            ICommonBusinessLogic commonBL,
            IScopeBusinessLogic scopeBL,
            ISubmissionBusinessLogic submissionBL,
            IOrganisationBusinessLogic orgBL,
            ISearchBusinessLogic searchBusinessLogic,
            IGovNotifyAPI govNotifyApi,
            UpdateFromCompaniesHouseService updateFromCompaniesHouseService,
            ISnapshotDateHelper snapshotDateHelper)
        {
            _CustomLogger = customLogger;
            _Messenger = messenger;
            DataRepository = dataRepository;
            FileRepository = fileRepository;
            _EmployerSearchRepository = employerSearchRepository;
            _SicCodeSearchRepository = sicCodeSearchRepository;
            _CommonBL = commonBL;
            _ScopeBL = scopeBL;
            _SubmissionBL = submissionBL;
            _OrganisationBL = orgBL;
            SearchBusinessLogic = searchBusinessLogic;
            _updateFromCompaniesHouseService = updateFromCompaniesHouseService;
            _snapshotDateHelper = snapshotDateHelper;
            this.govNotifyApi = govNotifyApi;
        }

        #region Properties

        private readonly ICustomLogger _CustomLogger;
        private readonly IMessenger _Messenger;
        public readonly IFileRepository FileRepository;
        public readonly IDataRepository DataRepository;
        private readonly ISearchRepository<EmployerSearchModel> _EmployerSearchRepository;
        private readonly ISearchRepository<SicCodeSearchModel> _SicCodeSearchRepository;
        private readonly IScopeBusinessLogic _ScopeBL;
        private readonly ICommonBusinessLogic _CommonBL;
        private readonly ISubmissionBusinessLogic _SubmissionBL;
        private readonly IOrganisationBusinessLogic _OrganisationBL;
        public readonly ISearchBusinessLogic SearchBusinessLogic;
        private readonly IGovNotifyAPI govNotifyApi;
        private readonly UpdateFromCompaniesHouseService _updateFromCompaniesHouseService;
        private readonly ISnapshotDateHelper _snapshotDateHelper;

        private static readonly ConcurrentSet<string> RunningJobs = new ConcurrentSet<string>();
        private static readonly ConcurrentSet<string> StartedJobs = new ConcurrentSet<string>();

        #endregion

        #region Timer Trigger Schedules

        public class EveryWorkingHourSchedule : WeeklySchedule
        {

            public EveryWorkingHourSchedule()
            {
                // Every hour on Monday 8am-7pm
                Add(DayOfWeek.Monday, new TimeSpan(8, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(9, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(10, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(11, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(12, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(13, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(14, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(15, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(16, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(17, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(18, 0, 0));
                Add(DayOfWeek.Monday, new TimeSpan(19, 0, 0));

                // Every hour on Tuesday 8am-7pm
                Add(DayOfWeek.Tuesday, new TimeSpan(8, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(9, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(10, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(11, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(12, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(13, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(14, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(15, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(16, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(17, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(18, 0, 0));
                Add(DayOfWeek.Tuesday, new TimeSpan(19, 0, 0));

                // Every hour on Wednesday 8am-7pm
                Add(DayOfWeek.Wednesday, new TimeSpan(8, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(9, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(10, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(11, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(12, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(13, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(14, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(15, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(16, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(17, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(18, 0, 0));
                Add(DayOfWeek.Wednesday, new TimeSpan(19, 0, 0));

                // Every hour on Thursday 8am-7pm
                Add(DayOfWeek.Thursday, new TimeSpan(8, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(9, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(10, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(11, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(12, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(13, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(14, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(15, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(16, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(17, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(18, 0, 0));
                Add(DayOfWeek.Thursday, new TimeSpan(19, 0, 0));

                // Every hour on Friday 8am-7pm
                Add(DayOfWeek.Friday, new TimeSpan(8, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(9, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(10, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(11, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(12, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(13, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(14, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(15, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(16, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(17, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(18, 0, 0));
                Add(DayOfWeek.Friday, new TimeSpan(19, 0, 0));
            }

        }

        public class OncePerWeekendRandomSchedule : WeeklySchedule
        {

            public OncePerWeekendRandomSchedule()
            {
                switch (Numeric.Rand(0, 59))
                {
                    case 0:
                        Add(DayOfWeek.Friday, new TimeSpan(20, 0, 0));
                        break;
                    case 1:
                        Add(DayOfWeek.Friday, new TimeSpan(21, 0, 0));
                        break;
                    case 2:
                        Add(DayOfWeek.Friday, new TimeSpan(22, 0, 0));
                        break;
                    case 3:
                        Add(DayOfWeek.Friday, new TimeSpan(23, 0, 0));
                        break;
                    case 4:
                        Add(DayOfWeek.Saturday, new TimeSpan(0, 0, 0));
                        break;
                    case 5:
                        Add(DayOfWeek.Saturday, new TimeSpan(1, 0, 0));
                        break;
                    case 6:
                        Add(DayOfWeek.Saturday, new TimeSpan(2, 0, 0));
                        break;
                    case 7:
                        Add(DayOfWeek.Saturday, new TimeSpan(3, 0, 0));
                        break;
                    case 8:
                        Add(DayOfWeek.Saturday, new TimeSpan(4, 0, 0));
                        break;
                    case 9:
                        Add(DayOfWeek.Saturday, new TimeSpan(5, 0, 0));
                        break;
                    case 10:
                        Add(DayOfWeek.Saturday, new TimeSpan(5, 0, 0));
                        break;
                    case 11:
                        Add(DayOfWeek.Saturday, new TimeSpan(7, 0, 0));
                        break;
                    case 12:
                        Add(DayOfWeek.Saturday, new TimeSpan(8, 0, 0));
                        break;
                    case 13:
                        Add(DayOfWeek.Saturday, new TimeSpan(9, 0, 0));
                        break;
                    case 14:
                        Add(DayOfWeek.Saturday, new TimeSpan(10, 0, 0));
                        break;
                    case 15:
                        Add(DayOfWeek.Saturday, new TimeSpan(11, 0, 0));
                        break;
                    case 16:
                        Add(DayOfWeek.Saturday, new TimeSpan(12, 0, 0));
                        break;
                    case 17:
                        Add(DayOfWeek.Saturday, new TimeSpan(13, 0, 0));
                        break;
                    case 18:
                        Add(DayOfWeek.Saturday, new TimeSpan(14, 0, 0));
                        break;
                    case 19:
                        Add(DayOfWeek.Saturday, new TimeSpan(15, 0, 0));
                        break;
                    case 20:
                        Add(DayOfWeek.Saturday, new TimeSpan(16, 0, 0));
                        break;
                    case 21:
                        Add(DayOfWeek.Saturday, new TimeSpan(17, 0, 0));
                        break;
                    case 22:
                        Add(DayOfWeek.Saturday, new TimeSpan(18, 0, 0));
                        break;
                    case 23:
                        Add(DayOfWeek.Saturday, new TimeSpan(19, 0, 0));
                        break;
                    case 24:
                        Add(DayOfWeek.Saturday, new TimeSpan(20, 0, 0));
                        break;
                    case 25:
                        Add(DayOfWeek.Saturday, new TimeSpan(21, 0, 0));
                        break;
                    case 26:
                        Add(DayOfWeek.Saturday, new TimeSpan(22, 0, 0));
                        break;
                    case 27:
                        Add(DayOfWeek.Saturday, new TimeSpan(23, 0, 0));
                        break;
                    case 28:
                        Add(DayOfWeek.Sunday, new TimeSpan(0, 0, 0));
                        break;
                    case 29:
                        Add(DayOfWeek.Sunday, new TimeSpan(1, 0, 0));
                        break;
                    case 30:
                        Add(DayOfWeek.Sunday, new TimeSpan(2, 0, 0));
                        break;
                    case 31:
                        Add(DayOfWeek.Sunday, new TimeSpan(3, 0, 0));
                        break;
                    case 32:
                        Add(DayOfWeek.Sunday, new TimeSpan(4, 0, 0));
                        break;
                    case 33:
                        Add(DayOfWeek.Sunday, new TimeSpan(5, 0, 0));
                        break;
                    case 34:
                        Add(DayOfWeek.Sunday, new TimeSpan(6, 0, 0));
                        break;
                    case 35:
                        Add(DayOfWeek.Sunday, new TimeSpan(7, 0, 0));
                        break;
                    case 36:
                        Add(DayOfWeek.Sunday, new TimeSpan(8, 0, 0));
                        break;
                    case 37:
                        Add(DayOfWeek.Sunday, new TimeSpan(9, 0, 0));
                        break;
                    case 38:
                        Add(DayOfWeek.Sunday, new TimeSpan(10, 0, 0));
                        break;
                    case 39:
                        Add(DayOfWeek.Sunday, new TimeSpan(11, 0, 0));
                        break;
                    case 40:
                        Add(DayOfWeek.Sunday, new TimeSpan(12, 0, 0));
                        break;
                    case 41:
                        Add(DayOfWeek.Sunday, new TimeSpan(13, 0, 0));
                        break;
                    case 42:
                        Add(DayOfWeek.Sunday, new TimeSpan(14, 0, 0));
                        break;
                    case 43:
                        Add(DayOfWeek.Sunday, new TimeSpan(15, 0, 0));
                        break;
                    case 44:
                        Add(DayOfWeek.Sunday, new TimeSpan(16, 0, 0));
                        break;
                    case 45:
                        Add(DayOfWeek.Sunday, new TimeSpan(17, 0, 0));
                        break;
                    case 46:
                        Add(DayOfWeek.Sunday, new TimeSpan(18, 0, 0));
                        break;
                    case 47:
                        Add(DayOfWeek.Sunday, new TimeSpan(19, 0, 0));
                        break;
                    case 48:
                        Add(DayOfWeek.Sunday, new TimeSpan(20, 0, 0));
                        break;
                    case 49:
                        Add(DayOfWeek.Sunday, new TimeSpan(21, 0, 0));
                        break;
                    case 50:
                        Add(DayOfWeek.Sunday, new TimeSpan(22, 0, 0));
                        break;
                    case 51:
                        Add(DayOfWeek.Sunday, new TimeSpan(23, 0, 0));
                        break;
                    case 52:
                        Add(DayOfWeek.Monday, new TimeSpan(0, 0, 0));
                        break;
                    case 53:
                        Add(DayOfWeek.Monday, new TimeSpan(1, 0, 0));
                        break;
                    case 54:
                        Add(DayOfWeek.Monday, new TimeSpan(2, 0, 0));
                        break;
                    case 55:
                        Add(DayOfWeek.Monday, new TimeSpan(3, 0, 0));
                        break;
                    case 56:
                        Add(DayOfWeek.Monday, new TimeSpan(4, 0, 0));
                        break;
                    case 57:
                        Add(DayOfWeek.Monday, new TimeSpan(5, 0, 0));
                        break;
                    case 58:
                        Add(DayOfWeek.Monday, new TimeSpan(6, 0, 0));
                        break;
                    case 59:
                        Add(DayOfWeek.Monday, new TimeSpan(7, 0, 0));
                        break;
                }
            }

        }


        public class MidnightSchedule : DailySchedule
        {

            public MidnightSchedule() : base("00:00:00") { }

        }

        #endregion

    }
}
