using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ModernSlavery.Core.Extensions
{
    public static class Enums
    {
        public static T GetEnumFromRange<T>(int min, int max) where T : Enum
        {
            foreach (T instance in Enum.GetValues(typeof(T)))
            {
                var range = instance.GetAttribute<RangeAttribute>();
                if (range != null && range.Minimum.ToInt32() == min && range.Maximum.ToInt32() == max) return instance;
            }
            return default(T);
        }
    }
}
