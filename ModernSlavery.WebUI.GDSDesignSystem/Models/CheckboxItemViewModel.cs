namespace ModernSlavery.WebUI.GDSDesignSystem.Models
{
    public class CheckboxItemViewModel : ItemViewModel
    {
        public override string StyleNamePrefix { get; } = "govuk-checkboxes";

        public override string InputType { get; } = "checkbox";
    }
}