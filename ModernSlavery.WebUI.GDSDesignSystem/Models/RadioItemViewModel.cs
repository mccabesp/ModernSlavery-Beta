namespace ModernSlavery.WebUI.GDSDesignSystem.Models
{
    public class RadioItemViewModel : ItemViewModel
    {
        public override string StyleNamePrefix { get; } = "govuk-radios";

        public override string InputType { get; } = "radio";
    }
}