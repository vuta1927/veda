using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using VDS.Helpers.Exception;
using JetBrains.Annotations;

namespace VDS.Helpers.Extensions
{
    /// <summary>
    /// Provides extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Concatenates the members of a constructed <see cref="IEnumerable{T}"/> collection of type System.String, using the specified separator between each member.
        /// This is a shortcut for string.Join(...)
        /// </summary>
        /// <param name="source">A collection that contains the strings to concatenate.</param>
        /// <param name="separator">The string to use as a separator. separator is included in the returned string only if values has more than one element.</param>
        /// <returns>A string that consists of the members of values delimited by the separator string. If values has no members, the method returns System.String.Empty.</returns>
        public static string JoinAsString(this IEnumerable<string> source, string separator)
        {
            return string.Join(separator, source);
        }

        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// This is a shortcut for string.Join(...)
        /// </summary>
        /// <param name="source">A collection that contains the objects to concatenate.</param>
        /// <param name="separator">The string to use as a separator. separator is included in the returned string only if values has more than one element.</param>
        /// <typeparam name="T">The type of the members of values.</typeparam>
        /// <returns>A string that consists of the members of values delimited by the separator string. If values has no members, the method returns System.String.Empty.</returns>
        public static string JoinAsString<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source);
        }
        
        /// <summary>
        /// Removes all leading and trailing occurrences of a set of strings specified
        /// in an array from the current System.String object.
        /// </summary>
        /// <param name="string">The string to format.</param>
        /// <param name="trimStrings">An array of strings to remove, or null.</param>
        /// <returns>
        /// The string that remains after all occurrences of the strings in the trimStrings
        /// parameter are removed from the start and end of the current string. If trimStrings
        /// is null or an empty array, white-space characters are removed instead.
        /// </returns>
        public static string Trim(this string @string, params string[] trimStrings)
        {
            return Trim(@string, StringComparison.CurrentCulture, trimStrings);
        }

        /// <summary>
        /// Removes all leading and trailing occurrences of a set of strings specified
        /// in an array from the current System.String object.
        /// </summary>
        /// <param name="string">The string to format.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how this string and trimStrings are compared.</param>
        /// <param name="trimStrings">An array of strings to remove, or null.</param>
        /// <returns>
        /// The string that remains after all occurrences of the strings in the trimStrings
        /// parameter are removed from the start and end of the current string. If trimStrings
        /// is null or an empty array, white-space characters are removed instead.
        /// </returns>
        public static string Trim(this string @string, StringComparison comparisonType, params string[] trimStrings)
        {
            Throw.IfArgumentNull(@string, nameof(@string));
            var result = @string;
            result = result.TrimStart(comparisonType, trimStrings);
            result = result.TrimEnd(comparisonType, trimStrings);
            return result;
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of strings specified in an array
        /// from the current System.String object.
        /// </summary>
        /// <param name="string">The string to format.</param>
        /// <param name="trimStrings">An array of strings to remove, or null.</param>
        /// <returns>
        /// The string that remains after all occurrences of the strings in the trimStrings
        /// parameter are removed from the end of the current string. If trimStrings
        /// is null or an empty array, white-space characters are removed instead.
        /// </returns>
        public static string TrimEnd(this string @string, params string[] trimStrings)
        {
            return TrimEnd(@string, StringComparison.CurrentCulture, trimStrings);
        }

        /// <summary>
        /// Removes all trailing occurrences of a set of strings specified in an array
        /// from the current System.String object.
        /// </summary>
        /// <param name="string">The string to format.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how this string and trimStrings are compared.</param>
        /// <param name="trimStrings">An array of strings to remove, or null.</param>
        /// <returns>
        /// The string that remains after all occurrences of the strings in the trimStrings
        /// parameter are removed from the end of the current string. If trimStrings
        /// is null or an empty array, white-space characters are removed instead.
        /// </returns>
        public static string TrimEnd(this string @string, StringComparison comparisonType, params string[] trimStrings)
        {
            Throw.IfArgumentNull(@string, nameof(@string));
            if (trimStrings.IsNullOrEmpty())
            {
                return @string.TrimEnd();
            }

            var str = "";
            var result = @string;

            for (int i = @string.Length - 1; i > 0; i--)
            {
                str = @string[i] + str;
                if (trimStrings.Any(s => s.Equals(str, comparisonType)))
                {
                    result = @string.Substring(0, i);
                    str = "";
                }
            }

            return result;
        }

        /// <summary>
        /// Removes all leading occurrences of a set of strings specified in an array
        /// from the current System.String object.
        /// </summary>
        /// <param name="string">The string to format.</param>
        /// <param name="trimStrings">An array of strings to remove, or null.</param>
        /// <returns>
        /// The string that remains after all occurrences of the strings in the trimStrings
        /// parameter are removed from the start of the current string. If trimStrings
        /// is null or an empty array, white-space characters are removed instead.
        /// </returns>
        public static string TrimStart(this string @string, params string[] trimStrings)
        {
            return TrimStart(@string, StringComparison.CurrentCulture, trimStrings);
        }

        /// <summary>
        /// Removes all leading occurrences of a set of strings specified in an array
        /// from the current System.String object.
        /// </summary>
        /// <param name="string">The string to format.</param>
        /// <param name="comparisonType">One of the enumeration values that determines how this string and trimStrings are compared.</param>
        /// <param name="trimStrings">An array of strings to remove, or null.</param>
        /// <returns>
        /// The string that remains after all occurrences of the strings in the trimStrings
        /// parameter are removed from the start of the current string. If trimStrings
        /// is null or an empty array, white-space characters are removed instead.
        /// </returns>
        public static string TrimStart(this string @string, StringComparison comparisonType, params string[] trimStrings)
        {
            Throw.IfArgumentNull(@string, nameof(@string));
            if (trimStrings.IsNullOrEmpty())
            {
                return @string.TrimStart();
            }

            var str = "";
            var result = @string;

            for (int i = 0; i < @string.Length; i++)
            {
                str = str + @string[i];
                if (trimStrings.Any(s => s.Equals(str, comparisonType)))
                {
                    result = @string.Substring(i + 1);
                    str = "";
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the current System.String object encloses by the specified strings.
        /// </summary>
        /// <param name="string">The current instance of string.</param>
        /// <param name="start">The string to compare to the substring at the start of this instance.</param>
        /// <param name="end">The string to compare to the substring at the end of this instance.</param>
        /// <returns>true if this instance encloses between start and end strings; otherwise, false.</returns>
        public static bool EnclosedBy(this string @string, string start, string end)
        {
            Throw.IfArgumentNull(@string, nameof(@string));
            return @string.StartsWith(start) && @string.EndsWith(end);
        }


        /// <summary>
        /// Determines whether this instance and another specified <see cref="String"/> object have the same value,
        /// using <see cref="StringComparison.OrdinalIgnoreCase"/> rule.
        /// </summary>
        /// <param name="string">The current instance of string.</param>
        /// <param name="value">The string to compare to this instance.</param>
        /// <returns>true if the value of the value parameter is the same as this instance; otherwise, false.</returns>
        public static bool EqualsIgnoreCase(this string @string, string value)
        {
            return string.Equals(@string, value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Replaces the format item in the current string instance with the string representation
        /// of a corresponding object in a specified array using invariant culture.
        /// </summary>
        /// <param name="string">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <returns></returns>
        [StringFormatMethod("string")]
        public static string FormatWith(this string @string, params object[] args)
        {
            return string.Format(@string, args);
        }

        public static string UnformatWith(this string @string, params string[] placeHolders)
        {
            var escapedString = @string.Replace("{", "{{").Replace("}", "}}");
            for (int i = 0; i < placeHolders.Length; i++)
            {
                escapedString = escapedString.Replace(placeHolders[i], "{" + i + "}");
            }
            return escapedString;
        }

        // From Orchard CMS
        public static byte[] ToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length).
                Where(x => 0 == x % 2).
                Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                ToArray();
        }

        // From Orchard CMS
        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        // From Orchard CMS
        public static string Ellipsize(this string text, int characterCount)
        {
            //return text.Ellipsize(characterCount, "&#160;&#8230;");
            return text.Ellipsize(characterCount, "...");
        }

        // From Orchard CMS
        public static string Ellipsize(this string text, int characterCount, string ellipsis)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            if (characterCount < 0 || text.Length <= characterCount)
                return text;

            return Regex.Replace(text.Substring(0, characterCount + 1), @"\s+\S*$", "") + ellipsis;
        }

        /// <summary>
        /// Indicates whether this string is null or an System.String.Empty string.
        /// </summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>
        /// indicates whether this string is null, empty, or consists only of white-space characters.
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Left(this string str, int len)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.Length < len)
            {
                throw new ArgumentException("len argument can not be bigger than given string's length!");
            }

            return str.Substring(0, len);
        }

        /// <summary>
        /// Converts line endings in the string to <see cref="Environment.NewLine"/>.
        /// </summary>
        public static string NormalizeLineEndings(this string str)
        {
            return str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
        }

        /// <summary>
        /// Removes first occurrence of the given postfixes from end of the given string.
        /// Ordering is important. If one of the postFixes is matched, others will not be tested.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="postFixes">one or more postfix.</param>
        /// <returns>Modified string or the same string if it has not any of given postfixes</returns>
        public static string RemovePostFix(this string str, params string[] postFixes)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty)
            {
                return string.Empty;
            }

            if (postFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var postFix in postFixes)
            {
                if (str.EndsWith(postFix))
                {
                    return str.Left(str.Length - postFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// Removes first occurrence of the given prefixes from beginning of the given string.
        /// Ordering is important. If one of the preFixes is matched, others will not be tested.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="preFixes">one or more prefix.</param>
        /// <returns>Modified string or the same string if it has not any of given prefixes</returns>
        public static string RemovePreFix(this string str, params string[] preFixes)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty)
            {
                return string.Empty;
            }

            if (preFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (var preFix in preFixes)
            {
                if (str.StartsWith(preFix))
                {
                    return str.Right(str.Length - preFix.Length);
                }
            }

            return str;
        }

        /// <summary>
        /// Gets a substring of a string from end of the string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="len"/> is bigger that string's length</exception>
        public static string Right(this string str, int len)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            if (str.Length < len)
            {
                throw new ArgumentException("len argument can not be bigger than given string's length!");
            }

            return str.Substring(str.Length - len, len);
        }

        /// <summary>
        /// Converts PascalCase string to camelCase string.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="invariantCulture">Invariant culture</param>
        /// <returns>camelCase of the string</returns>
        public static string ToCamelCase(this string str, bool invariantCulture = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return invariantCulture ? str.ToLowerInvariant() : str.ToLower();
            }

            return (invariantCulture ? char.ToLowerInvariant(str[0]) : char.ToLower(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// Converts PascalCase string to camelCase string in specified culture.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="culture">An object that supplies culture-specific casing rules</param>
        /// <returns>camelCase of the string</returns>
        public static string ToCamelCase(this string str, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return str.ToLower(culture);
            }

            return char.ToLower(str[0], culture) + str.Substring(1);
        }

        /// <summary>
        /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
        /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="invariantCulture">Invariant culture</param>
        public static string ToSentenceCase(this string str, bool invariantCulture = false)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return Regex.Replace(
                str,
                "[a-z][A-Z]",
                m => m.Value[0] + " " + (invariantCulture ? char.ToLowerInvariant(m.Value[1]) : char.ToLower(m.Value[1]))
            );
        }

        /// <summary>
        /// Converts given PascalCase/camelCase string to sentence (by splitting words by space).
        /// Example: "ThisIsSampleSentence" is converted to "This is a sample sentence".
        /// </summary>
        /// <param name="str">String to convert.</param>
        /// <param name="culture">An object that supplies culture-specific casing rules.</param>
        public static string ToSentenceCase(this string str, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            return Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1], culture));
        }

        /// <summary>
        /// Converts string to enum value.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String value to convert</param>
        /// <returns>Returns enum object</returns>
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Converts string to enum value.
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <param name="value">String value to convert</param>
        /// <param name="ignoreCase">Ignore case</param>
        /// <returns>Returns enum object</returns>
        public static T ToEnum<T>(this string value, bool ignoreCase)
            where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        public static string ToMd5(this string str)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(str);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (var hashByte in hashBytes)
                {
                    sb.Append(hashByte.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Converts camelCase string to PascalCase string.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="invariantCulture">Invariant culture</param>
        /// <returns>PascalCase of the string</returns>
        public static string ToPascalCase(this string str, bool invariantCulture = true)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return invariantCulture ? str.ToUpperInvariant() : str.ToUpper();
            }

            return (invariantCulture ? char.ToUpperInvariant(str[0]) : char.ToUpper(str[0])) + str.Substring(1);
        }

        /// <summary>
        /// Converts camelCase string to PascalCase string in specified culture.
        /// </summary>
        /// <param name="str">String to convert</param>
        /// <param name="culture">An object that supplies culture-specific casing rules</param>
        /// <returns>PascalCase of the string</returns>
        public static string ToPascalCase(this string str, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return str;
            }

            if (str.Length == 1)
            {
                return str.ToUpper(culture);
            }

            return char.ToUpper(str[0], culture) + str.Substring(1);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string Truncate(this string str, int maxLength)
        {
            if (str == null)
            {
                return null;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            return str.Left(maxLength);
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// It adds a "..." postfix to end of the string if it's truncated.
        /// Returning string can not be longer than maxLength.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string TruncateWithPostfix(this string str, int maxLength)
        {
            return TruncateWithPostfix(str, maxLength, "...");
        }

        /// <summary>
        /// Gets a substring of a string from beginning of the string if it exceeds maximum length.
        /// It adds given <paramref name="postfix"/> to end of the string if it's truncated.
        /// Returning string can not be longer than maxLength.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="str"/> is null</exception>
        public static string TruncateWithPostfix(this string str, int maxLength, string postfix)
        {
            if (str == null)
            {
                return null;
            }

            if (str == string.Empty || maxLength == 0)
            {
                return string.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }

            if (maxLength <= postfix.Length)
            {
                return postfix.Left(maxLength);
            }

            return str.Left(maxLength - postfix.Length) + postfix;
        }
    }
}