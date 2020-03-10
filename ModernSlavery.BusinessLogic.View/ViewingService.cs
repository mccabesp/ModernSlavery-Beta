using System;
using System.Collections.Generic;
using System.Text;
using ModernSlavery.SharedKernel.Interfaces;

namespace ModernSlavery.BusinessLogic.View
{
    public interface IViewingService
    {
        ICommonBusinessLogic CommonBusinessLogic { get; }
        IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        ISubmissionBusinessLogic SubmissionBusinessLogic { get; set; }
        ISearchBusinessLogic SearchBusinessLogic { get; set; }

        IObfuscator Obfuscator { get; }

    }
    public class ViewingService: IViewingService
    {
        public ViewingService(
            IOrganisationBusinessLogic organisationBusinessLogic,
            ICommonBusinessLogic commonBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            ISearchBusinessLogic searchBusinessLogic,
            IObfuscator obfuscator)
        {
            OrganisationBusinessLogic = organisationBusinessLogic;
            CommonBusinessLogic = commonBusinessLogic;
            SubmissionBusinessLogic = submissionBusinessLogic;
            SearchBusinessLogic = searchBusinessLogic;
            Obfuscator = obfuscator;
        }

        public ICommonBusinessLogic CommonBusinessLogic { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; set; }
        public ISearchBusinessLogic SearchBusinessLogic { get; set; }
        public IObfuscator Obfuscator { get; }
    }
}
