using System;
using System.Text;

namespace NzbDrone.Core.Parser.RomanNumerals
{
    /// <summary>
    /// Represents the numeric system used in ancient Rome, employing combinations of letters from the Latin alphabet to signify values.
    /// Implementation adapted from: http://www.c-sharpcorner.com/Blogs/14255/converting-to-and-from-roman-numerals.aspx
    /// </summary>
    public class RomanNumeral : IComparable, IComparable<RomanNumeral>, IEquatable<RomanNumeral>, IRomanNumeral
    {
        /// <summary>
        /// The numeric value of the roman numeral.
        /// </summary>
        private readonly int _value;

        /// <summary>
        /// Represents the smallest possible value of an <see cref="T:RomanNumeral"/>. This field is constant.
        /// </summary>
        public static readonly int MinValue = 1;

        /// <summary>
        /// Represents the largest possible value of an <see cref="T:RomanNumeral"/>. This field is constant.
        /// </summary>
        public static readonly int MaxValue = 3999;

        private static readonly string[] Thousands = { "MMM", "MM", "M" };
        private static readonly string[] Hundreds = { "CM", "DCCC", "DCC", "DC", "D", "CD", "CCC", "CC", "C" };
        private static readonly string[] Tens = { "XC", "LXXX", "LXX", "LX", "L", "XL", "XXX", "XX", "X" };
        private static readonly string[] Units = { "IX", "VIII", "VII", "VI", "V", "IV", "III", "II", "I" };

        /// <summary>
        /// Initializes a new instance of the <see cref="RomanNumeral"/> class.
        /// </summary>
        public RomanNumeral()
        {
            _value = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RomanNumeral"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public RomanNumeral(int value)
        {
            _value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RomanNumeral"/> class.
        /// </summary>
        /// <param name="romanNumeral">The roman numeral.</param>
        public RomanNumeral(string romanNumeral)
        {
            if (TryParse(romanNumeral, out var value))
            {
                _value = value;
            }
        }

        /// <summary>
        /// Converts this instance to an integer.
        /// </summary>
        /// <returns>A numeric int representation.</returns>
        public int ToInt()
        {
            return _value;
        }

        /// <summary>
        /// Converts this instance to a long.
        /// </summary>
        /// <returns>A numeric long representation.</returns>
        public long ToLong()
        {
            return _value;
        }

        /// <summary>
        /// Converts the string representation of a number to its 32-bit signed integer equivalent. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="text">A string containing a number to convert. </param>
        /// <param name="value">When this method returns, contains the 32-bit signed integer value equivalent of the number contained in <paramref name="text"/>,
        /// if the conversion succeeded, or zero if the conversion failed. The conversion fails if the <paramref name="text"/> parameter is null  or
        /// <see cref="F:System.String.Empty"/>, is not of the correct format, or represents a number less than <see cref="F:System.Int32.MinValue"/> or greater than <see cref="F:System.Int32.MaxValue"/>. This parameter is passed uninitialized. </param><filterpriority>1</filterpriority>
        /// <returns>
        /// true if <paramref name="text"/> was converted successfully; otherwise, false.
        /// </returns>
        public static bool TryParse(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            text = text.ToUpper();
            var len = 0;

            for (var i = 0; i < 3; i++)
            {
                if (text.StartsWith(Thousands[i]))
                {
                    value += 1000 * (3 - i);
                    len = Thousands[i].Length;
                    break;
                }
            }

            if (len > 0)
            {
                text = text.Substring(len);
                len = 0;
            }

            for (var i = 0; i < 9; i++)
            {
                if (text.StartsWith(Hundreds[i]))
                {
                    value += 100 * (9 - i);
                    len = Hundreds[i].Length;
                    break;
                }
            }

            if (len > 0)
            {
                text = text.Substring(len);
                len = 0;
            }

            for (var i = 0; i < 9; i++)
            {
                if (text.StartsWith(Tens[i]))
                {
                    value += 10 * (9 - i);
                    len = Tens[i].Length;
                    break;
                }
            }

            if (len > 0)
            {
                text = text.Substring(len);
                len = 0;
            }

            for (var i = 0; i < 9; i++)
            {
                if (text.StartsWith(Units[i]))
                {
                    value += 9 - i;
                    len = Units[i].Length;
                    break;
                }
            }

            if (text.Length > len)
            {
                value = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a number into a roman numeral.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        private static string ToRomanNumeral(int number)
        {
            RangeGuard(number);
            int thousands, hundreds, tens, units;
            thousands = number / 1000;
            number %= 1000;
            hundreds = number / 100;
            number %= 100;
            tens = number / 10;
            units = number % 10;
            var sb = new StringBuilder();
            if (thousands > 0)
            {
                sb.Append(Thousands[3 - thousands]);
            }

            if (hundreds > 0)
            {
                sb.Append(Hundreds[9 - hundreds]);
            }

            if (tens > 0)
            {
                sb.Append(Tens[9 - tens]);
            }

            if (units > 0)
            {
                sb.Append(Units[9 - units]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Returns the Roman numeral that was passed in as either an Arabic numeral
        /// or a Roman numeral.
        /// </summary>
        /// <returns>A <see cref="string" /> representing a Roman Numeral</returns>
        public string ToRomanNumeral()
        {
            return ToString();
        }

        /// <summary>
        /// Determines whether a given number is within the valid range of values for a roman numeral.
        /// </summary>
        /// <param name="number">The number to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// $Roman numerals can not be larger than {MaxValue}.
        /// or
        /// $Roman numerals can not be smaller than {MinValue}.
        /// </exception>
        private static void RangeGuard(int number)
        {
            if (number > MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(number), number, $"Roman numerals can not be larger than {MaxValue}.");
            }

            if (number < MinValue)
            {
                throw new ArgumentOutOfRangeException(nameof(number), number, $"Roman numerals can not be smaller than {MinValue}.");
            }
        }

        /// <summary>
        /// Implements the operator *.
        /// </summary>
        /// <param name="firstNumeral">The first numeral.</param>
        /// <param name="secondNumeral">The second numeral.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static RomanNumeral operator *(RomanNumeral firstNumeral, RomanNumeral secondNumeral)
        {
            return new RomanNumeral(firstNumeral._value * secondNumeral._value);
        }

        /// <summary>
        /// Implements the operator /.
        /// </summary>
        /// <param name="numerator">The numerator.</param>
        /// <param name="denominator">The denominator.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static RomanNumeral operator /(RomanNumeral numerator, RomanNumeral denominator)
        {
            return new RomanNumeral(numerator._value / denominator._value);
        }

        /// <summary>
        /// Implements the operator +.
        /// </summary>
        /// <param name="firstNumeral">The first numeral.</param>
        /// <param name="secondNumeral">The second numeral.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static RomanNumeral operator +(RomanNumeral firstNumeral, RomanNumeral secondNumeral)
        {
            return new RomanNumeral(firstNumeral._value + secondNumeral._value);
        }

        /// <summary>
        /// Implements the operator -.
        /// </summary>
        /// <param name="firstNumeral">The first numeral.</param>
        /// <param name="secondNumeral">The second numeral.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static RomanNumeral operator -(RomanNumeral firstNumeral, RomanNumeral secondNumeral)
        {
            return new RomanNumeral(firstNumeral._value - secondNumeral._value);
        }

        /// <summary>
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj is sbyte
                || obj is byte
                || obj is short
                || obj is ushort
                || obj is int
                || obj is uint
                || obj is long
                || obj is ulong
                || obj is float
                || obj is double
                || obj is decimal)
            {
                var value = (int)obj;
                return _value.CompareTo(value);
            }
            else if (obj is string)
            {
                var numeral = obj as string;

                if (TryParse(numeral, out var value))
                {
                    return _value.CompareTo(value);
                }
            }

            return 0;
        }

        /// <summary>
        /// Compares to.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(RomanNumeral other)
        {
            return _value.CompareTo(other._value);
        }

        /// <summary>
        /// Equalses the specified other.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public bool Equals(RomanNumeral other)
        {
            return _value == other._value;
        }

        /// <summary>
        /// Returns the Roman Numeral which was passed to this Instance
        /// during creation.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents a Roman Numeral.
        /// </returns>
        public override string ToString()
        {
            return ToRomanNumeral(_value);
        }
    }
}
