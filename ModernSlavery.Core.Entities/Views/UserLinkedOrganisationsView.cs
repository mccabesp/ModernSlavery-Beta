namespace ModernSlavery.Core.Entities.Views
{
    public class UserLinkedOrganisationsView
    {
        public long UserId { get; set; }
        public long OrganisationId { get; set; }
        public string Dunsnumber { get; set; }
        public string EmployerReference { get; set; }
        public string CompanyNumber { get; set; }
        public string OrganisationName { get; set; }
        public string SectorTypeId { get; set; }
        public string StatusId { get; set; }
    }
}