namespace ModernSlavery.Entities
{
    public class OrganisationSicCodeInfoView
    {
        public long OrganisationId { get; set; }
        public int? SicCodeId { get; set; }
        public long? SicCodeRankWithinOrganisation { get; set; }
        public string Source { get; set; }
        public string CodeDescription { get; set; }
        public string SicSectionId { get; set; }
        public string SectionDescription { get; set; }
    }
}