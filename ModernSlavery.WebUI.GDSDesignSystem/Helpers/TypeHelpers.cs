using System;
using System.Collections.Generic;

namespace ModernSlavery.WebUI.GDSDesignSystem.Helpers
{
    public static class TypeHelpers
    {
        public static bool IsNullableEnum(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);

            var isNullableType = underlyingType != null;

            var isNullableEnum = isNullableType && underlyingType.IsEnum;

            return isNullableEnum;
        }

        public static bool IsListOfEnums(Type type)
        {
            var isGenericList = type.IsGenericType &&
                                type.GetGenericTypeDefinition() == typeof(List<>);

            if (!isGenericList) return false;

            var genericType = type.GetGenericArguments()[0];
            return genericType.IsEnum;
        }

        public static Type GetGenericTypeFromGenericListType(Type listType)
        {
            return listType.GetGenericArguments()[0];
        }

        public static object GetDefaultValue(Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type);

            return null;
        }
    }
}