using System;

namespace ModernSlavery.WebUI.GDSDesignSystem.Attributes.ValidationAttributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GovUkValidateCharacterCountAttribute : Attribute
    {
        public int MaxCharacters { get; set; }
    }
}