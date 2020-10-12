using System;
using ModernSlavery.Core.Extensions;

namespace ModernSlavery.Core.Entities
{
    [Serializable]
    public class PublicSectorType
    {
        public int PublicSectorTypeId { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public override bool Equals(object obj)
        {
            var target = obj as PublicSectorType;
            if (target == null) return false;

            return PublicSectorTypeId == target.PublicSectorTypeId;
        }
    }
}