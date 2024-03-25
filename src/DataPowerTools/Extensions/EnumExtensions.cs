using System;

namespace DataPowerTools.Extensions
{
    /// <summary>
    /// Provides useful extensions for enumerations.
    /// </summary>
    public static class EnumExtensions
    {
        public static T ParseEnumType<T>(this string enumString)
        {
            return (T) Enum.Parse(typeof(T), enumString);
        }
        
        //public static string WriteEnumAsString<TEnum>(TEnum tEnum) where TEnum : Enum
        //{
        //    return tEnum.ToString();
        //}

        //public static string[] GetEnumOptionsAsStringArray<TEnum>(this TEnum t) where TEnum : Enum
        //{
        //    return Enum.GetNames(typeof(TEnum));
        //}
    }
}
