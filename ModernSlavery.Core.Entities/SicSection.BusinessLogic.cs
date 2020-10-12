using System;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    public partial class SicSection
    {
        public override bool Equals(object obj)
        {
            var target = obj as SicSection;
            if (target == null) return false;

            return SicSectionId == target.SicSectionId;
        }
    }
}