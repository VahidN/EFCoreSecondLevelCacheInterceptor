using System;
using System.Collections.Generic;
using System.Linq;

namespace EFCoreSecondLevelCacheInterceptor;

/// <summary>
///     Missing NET4_6_2 exts
/// </summary>
internal static class StringExtensions
{
    /// <summary>
    ///     Determines if a collection contains an item which ends with the given value
    /// </summary>
    public static bool EndsWith(this IEnumerable<string>? collection, string? value,
                                StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return collection?.Any(item => item.EndsWith(value, stringComparison)) == true;
    }

    /// <summary>
    ///     Determines if a collection contains an item which starts with the given value
    /// </summary>
    public static bool StartsWith(this IEnumerable<string>? collection, string? value,
                                  StringComparison stringComparison)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return collection?.Any(item => item.StartsWith(value, stringComparison)) == true;
    }

    /// <summary>
    ///     Determines if a collection exclusively contains every item in the given collection
    /// </summary>
    public static bool ContainsEvery(this IEnumerable<string>? source, IEnumerable<string>? collection,
                                     StringComparer stringComparison)
    {
        if (source is null || collection is null)
        {
            return false;
        }

        return source.OrderBy(fElement => fElement, stringComparison).SequenceEqual(
         collection.OrderBy(sElement => sElement, stringComparison),
         stringComparison);
    }

    /// <summary>
    ///     Determines if a collection contains items only in the given collection
    /// </summary>
    public static bool ContainsOnly(this IEnumerable<string>? source, IEnumerable<string>? collection,
                                    StringComparer stringComparison)
    {
        if (source is null || collection is null)
        {
            return false;
        }

        return source.All(sElement => collection.Contains(sElement, stringComparison));
    }


#if NET4_6_2 || NETSTANDARD2_0
        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.
        /// </summary>
        public static bool Contains(this string commandText, string value, StringComparison comparisonType) =>
            !string.IsNullOrWhiteSpace(commandText) && commandText.IndexOf(value, comparisonType) >= 0;

        /// <summary>
        /// Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string, using the provided comparison type.
        /// </summary>
        public static string Replace(this string str, string oldValue, string newValue, StringComparison comparisonType)
        {
            newValue = newValue ?? string.Empty;
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(oldValue) || oldValue.Equals(newValue, comparisonType))
            {
                return str;
            }
            int foundAt;
            while ((foundAt = str.IndexOf(oldValue, 0, comparisonType)) != -1)
            {
                str = str.Remove(foundAt, oldValue.Length).Insert(foundAt, newValue);
            }
            return str;
        }

        /// <summary>
        /// Returns the hash code for this string using the specified rules.
        /// </summary>
        public static int GetHashCode(this string str, StringComparison comparisonType)
        {
            return comparisonType switch
            {
                StringComparison.CurrentCulture => StringComparer.CurrentCulture.GetHashCode(str),
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase.GetHashCode(str),
                StringComparison.InvariantCulture => StringComparer.InvariantCulture.GetHashCode(str),
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase.GetHashCode(str),
                StringComparison.Ordinal => StringComparer.Ordinal.GetHashCode(str),
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase.GetHashCode(str),
                _ => throw new NotSupportedException(),
            };
        }
#endif
}