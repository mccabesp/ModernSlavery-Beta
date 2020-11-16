using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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

        public static IEnumerable<T> GetValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static IEnumerable<T> GetValuesExcept<T>(params T[] exceptions) where T : Enum
            => GetValues<T>().Except(exceptions);

        /// <summary>
        /// Returns the description text provided by a DescriptionAttribute or DisplayAttribute
        /// If neigther of these attributes or the text is null then the name of the enum is returned as a string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(this T value) where T : Enum
        {
            if (value==null || value.Equals(default(T))) throw new ArgumentNullException(nameof(value));

            var description = value.GetAttribute<DescriptionAttribute>()?.Description;
            if (string.IsNullOrWhiteSpace(description)) description=value.GetAttribute<DisplayAttribute>()?.Description;
            if (string.IsNullOrWhiteSpace(description)) description= value.ToString();
            return description;
        }
    }
}
