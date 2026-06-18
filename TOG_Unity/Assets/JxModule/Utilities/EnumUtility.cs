using System;
using System.Collections.Generic;

namespace JxModule
{
    public static class EnumUtility
    {
        public static TEnum[] GetValues<TEnum>() where TEnum : Enum
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        public static List<TEnum> GetValuesList<TEnum>() where TEnum : Enum
        {
            return new List<TEnum>(GetValues<TEnum>());
        }

        public static bool IsDefined<TEnum>(TEnum value) where TEnum : Enum
        {
            return Enum.IsDefined(typeof(TEnum), value);
        }

        public static bool TryParse<TEnum>(string value, out TEnum result, bool ignoreCase = true) where TEnum : struct, Enum
        {
            return Enum.TryParse(value, ignoreCase, out result) && IsDefined(result);
        }

        public static TEnum ParseOrDefault<TEnum>(string value, TEnum defaultValue = default, bool ignoreCase = true) where TEnum : struct, Enum
        {
            return TryParse(value, out TEnum result, ignoreCase) ? result : defaultValue;
        }

        public static TEnum GetRandom<TEnum>() where TEnum : Enum
        {
            var values = GetValues<TEnum>();
            return RandomUtility.GetRandom(values);
        }

        public static bool HasAnyFlag<TEnum>(TEnum value, TEnum flag) where TEnum : struct, Enum
        {
            var valueBits = Convert.ToUInt64(value);
            var flagBits = Convert.ToUInt64(flag);
            
            return (valueBits & flagBits) != 0;
        }

        public static bool HasAllFlags<TEnum>(TEnum value, TEnum flag) where TEnum : struct, Enum
        {
            var valueBits = Convert.ToUInt64(value);
            var flagBits = Convert.ToUInt64(flag);
            
            return (valueBits & flagBits) == flagBits;
        }
    }
}