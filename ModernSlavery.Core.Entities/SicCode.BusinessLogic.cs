using System;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    public partial class SicCode
    {
        public override bool Equals(object obj)
        {
            var target = obj as SicCode;
            if (target == null) return false;

            return SicCodeId == target.SicCodeId;
        }
    }
}