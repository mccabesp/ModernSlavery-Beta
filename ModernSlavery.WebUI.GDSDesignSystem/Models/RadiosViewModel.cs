namespace ModernSlavery.WebUI.GDSDesignSystem.Models
{
    public class RadiosViewModel : ItemSetViewModel
    {
        public override string StyleNamePrefix { get; } = "govuk-radios";
        public override string ItemDesignFileName { get; } = "/Components/RadioItem.cshtml";
    }
}