using System;
using System.Globalization;

namespace FluentCommand.Extensions
{
    /// <summary>
    /// Converts a string data type to another base data type using a safe conversion method.
    /// </summary>
    public static class ConvertExtensions
    {
        /// <summary>
        /// Converts the specified string representation of a logical value to its Boolean equivalent.
        /// </summary>
        /// <param name="value">A string that contains the value of either <see cref="F:System.Boolean.TrueString"/> or <see cref="F:System.Boolean.FalseString"/>.</param>
        /// <returns>
        /// true if <paramref name="value"/> equals <see cref="F:System.Boolean.TrueString"/>, or false if <paramref name="value"/> equals <see cref="F:System.Boolean.FalseString"/> or null.
        /// </returns>
        public static bool ToBoolean(this string value)
        {
            if (value == null)
                return false;

            bool result;
            if (bool.TryParse(value, out result))
                return result;

            string v = value.Trim();

            if (string.Equals(v, "t", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "x", StringComparison.OrdinalIgnoreCase)
                || string.Equals(v, "on", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 8-bit unsigned integer.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// An 8-bit unsigned integer that is equivalent to <paramref name="value"/>, or zero if <paramref name="value"/> is null.
        /// </returns>
        public static byte ToByte(this string value)
        {
            if (value == null)
                return 0;

            byte result;
            byte.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 8-bit unsigned integer, using specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// An 8-bit unsigned integer that is equivalent to <paramref name="value"/>, or zero if <paramref name="value"/> is null.
        /// </returns>
        public static byte ToByte(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0;

            byte result;
            Byte.TryParse(value, NumberStyles.Integer, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a date and time to an equivalent date and time value.
        /// </summary>
        /// <param name="value">The string representation of a date and time.</param>
        /// <returns>
        /// The date and time equivalent of the value of <paramref name="value"/>, or the date and time equivalent of <see cref="F:System.DateTime.MinValue"/> if <paramref name="value"/> is null.
        /// </returns>
        public static DateTime ToDateTime(this string value)
        {
            if (value == null)
                return new DateTime(0L);

            DateTime result;
            if (DateTime.TryParse(value, out result))
                return result;

            if (DateTime.TryParseExact(value, "M/d/yyyy hh:mm:ss tt", CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
                return result;

            if (DateTime.TryParseExact(value, "M/d/yyyy", CultureInfo.CurrentCulture, DateTimeStyles.None, out result))
                return result;

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent date and time, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains a date and time to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// The date and time equivalent of the value of <paramref name="value"/>, or the date and time equivalent of <see cref="F:System.DateTime.MinValue"/> if <paramref name="value"/> is null.
        /// </returns>
        public static DateTime ToDateTime(this string value, IFormatProvider provider)
        {
            if (value == null)
                return new DateTime(0L);

            DateTime result;
            if (DateTime.TryParse(value, out result))
                return result;

            if (DateTime.TryParseExact(value, "M/d/yyyy hh:mm:ss tt", provider, DateTimeStyles.None, out result))
                return result;

            if (DateTime.TryParseExact(value, "M/d/yyyy", provider, DateTimeStyles.None, out result))
                return result;

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent decimal number.
        /// </summary>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <returns>
        /// A decimal number that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static decimal ToDecimal(this string value)
        {
            if (value == null)
                return 0M;

            decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out var result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent decimal number, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A decimal number that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static decimal ToDecimal(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0M;

            decimal.TryParse(value, NumberStyles.Currency, provider, out var result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent double-precision floating-point number.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// A double-precision floating-point number that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static double ToDouble(this string value)
        {
            if (value == null)
                return 0.0;

            double result;
            double.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent double-precision floating-point number, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A double-precision floating-point number that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static double ToDouble(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0.0;

            double result;
            double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 16-bit signed integer.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// A 16-bit signed integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static short ToInt16(this string value)
        {
            if (value == null)
                return 0;

            short result;
            short.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 16-bit signed integer, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A 16-bit signed integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static short ToInt16(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0;

            short result;
            short.TryParse(value, NumberStyles.Integer, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 32-bit signed integer.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// A 32-bit signed integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static int ToInt32(this string value)
        {
            if (value == null)
                return 0;

            int result;
            int.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 32-bit signed integer, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A 32-bit signed integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static int ToInt32(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0;

            int result;
            int.TryParse(value, NumberStyles.Integer, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 64-bit signed integer.
        /// </summary>
        /// <param name="value">A string that contains a number to convert.</param>
        /// <returns>
        /// A 64-bit signed integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static long ToInt64(this string value)
        {
            if (value == null)
                return 0L;

            long result;
            long.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 64-bit signed integer, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A 64-bit signed integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static long ToInt64(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0L;

            long result;
            long.TryParse(value, NumberStyles.Integer, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent single-precision floating-point number.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// A single-precision floating-point number that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static float ToSingle(this string value)
        {
            if (value == null)
                return 0f;

            float result;
            float.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent single-precision floating-point number, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A single-precision floating-point number that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static float ToSingle(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0F;

            float result;
            float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// A 16-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static ushort ToUInt16(this string value)
        {
            if (value == null)
                return 0;

            ushort result;
            ushort.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 16-bit unsigned integer, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A 16-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static ushort ToUInt16(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0;

            ushort result;
            ushort.TryParse(value, NumberStyles.Integer, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// A 32-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static uint ToUInt32(this string value)
        {
            if (value == null)
                return 0;

            uint result;
            uint.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 32-bit unsigned integer, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A 32-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static uint ToUInt32(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0;

            uint result;
            uint.TryParse(value, NumberStyles.Integer, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 64-bit unsigned integer.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <returns>
        /// A 64-bit signed integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static ulong ToUInt64(this string value)
        {
            if (value == null)
                return 0L;

            ulong result;
            ulong.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string representation of a number to an equivalent 64-bit unsigned integer, using the specified culture-specific formatting information.
        /// </summary>
        /// <param name="value">A string that contains the number to convert.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>
        /// A 64-bit unsigned integer that is equivalent to the number in <paramref name="value"/>, or 0 (zero) if <paramref name="value"/> is null.
        /// </returns>
        public static ulong ToUInt64(this string value, IFormatProvider provider)
        {
            if (value == null)
                return 0L;

            ulong result;
            ulong.TryParse(value, NumberStyles.Integer, provider, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string to an equivalent <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The string representation of a <see cref="TimeSpan"/>.</param>
        /// <returns>
        /// The <see cref="TimeSpan"/> equivalent of the <paramref name="value"/>, or <see cref="F:System.TimeSpan.Zero"/> if <paramref name="value"/> is null.
        /// </returns>
        public static TimeSpan ToTimeSpan(this string value)
        {
            if (value == null)
                return TimeSpan.Zero;

            TimeSpan result;
            TimeSpan.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Converts the specified string to an equivalent <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The string representation of a <see cref="Guid"/>.</param>
        /// <returns>
        /// The <see cref="Guid"/> equivalent of the <paramref name="value"/>, or <see cref="F:System.Guid.Empty"/> if <paramref name="value"/> is null.
        /// </returns>
        public static Guid ToGuid(this string value)
        {
            if (value == null)
                return Guid.Empty;

            Guid result;
            Guid.TryParse(value, out result);

            return result;
        }

        /// <summary>
        /// Tries to convert the <paramref name="input"/> to the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="input">The input to convert.</param>
        /// <param name="type">The type to convert to.</param>
        /// <param name="value">The converted value.</param>
        /// <returns><c>true</c> if the vaule was converted; otherwise <c>false</c>.</returns>
        public static bool TryConvert(this string input, Type type, out object value)
        {
            // first try string
            if (type == typeof(string))
            {
                value = input;
                return true;
            }

            // check nullable
            if ((input == null || input.IsNullOrEmpty()) && type.IsNullable())
            {
                value = null;
                return true;
            }

            input = input?.Trim();

            Type underlyingType = type.GetUnderlyingType();

            // convert by type
            if (underlyingType == typeof(bool))
            {
                value = input.ToBoolean();
                return true;
            }
            if (underlyingType == typeof(byte))
            {
                value = input.ToByte();
                return true;
            }
            if (underlyingType == typeof(DateTime))
            {
                value = input.ToDateTime();
                return true;
            }
            if (underlyingType == typeof(decimal))
            {
                value = input.ToDecimal();
                return true;
            }
            if (underlyingType == typeof(double))
            {
                value = input.ToDouble();
                return true;
            }
            if (underlyingType == typeof(short))
            {
                value = input.ToInt16();
                return true;
            }
            if (underlyingType == typeof(int))
            {
                value = input.ToInt32();
                return true;
            }
            if (underlyingType == typeof(long))
            {
                value = input.ToInt64();
                return true;
            }
            if (underlyingType == typeof(float))
            {
                value = input.ToSingle();
                return true;
            }
            if (underlyingType == typeof(ushort))
            {
                value = input.ToUInt16();
                return true;
            }
            if (underlyingType == typeof(uint))
            {
                value = input.ToUInt32();
                return true;
            }
            if (underlyingType == typeof(ulong))
            {
                value = input.ToUInt64();
                return true;
            }
            if (underlyingType == typeof(TimeSpan))
            {
                value = input.ToTimeSpan();
                return true;
            }
            if (underlyingType == typeof(Guid))
            {
                value = input.ToGuid();
                return true;
            }

            value = default;
            return false;
        }


        /// <summary>
        /// Converts the result to the TValue type.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="result">The result to convert.</param>
        /// <param name="convert">The optional convert function.</param>
        /// <returns>The converted value.</returns>
        public static TValue ConvertValue<TValue>(this object result, Func<object, TValue> convert = null)
        {
            TValue value;

            if (result == null || result == DBNull.Value)
                value = default(TValue);
            else if (result is TValue)
                value = (TValue)result;
            else if (convert != null)
                value = convert(result);
            else
                value = (TValue)Convert.ChangeType(result, typeof(TValue));

            return value;
        }


    }
}
