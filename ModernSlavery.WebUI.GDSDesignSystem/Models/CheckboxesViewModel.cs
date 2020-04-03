namespace ModernSlavery.WebUI.GDSDesignSystem.Models
{
    public class CheckboxesViewModel : ItemSetViewModel
    {
        public override string StyleNamePrefix { get; } = "govuk-checkboxes";
        public override string ItemDesignFileName { get; } = "/Partials/CheckboxItem.cshtml";
    }
}