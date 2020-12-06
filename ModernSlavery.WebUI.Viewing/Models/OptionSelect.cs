using System;

namespace ModernSlavery.WebUI.Viewing.Models
{
    [Serializable]
    public class OptionSelect
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public bool Checked { get; set; }
        public bool Disabled { get; set; }
        public string Value { get; set; }
    }
}