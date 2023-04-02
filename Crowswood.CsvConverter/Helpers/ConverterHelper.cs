using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Helpers
{
    /// <summary>
    /// Static helper class to help with <see cref="Converter"/> functionality.
    /// </summary>
    internal static class ConverterHelper
    {
        /// <summary>
        /// Converts and returns the values of the specified <paramref name="items"/> as strings 
        /// according to their <see cref="Type"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="Type"/> and <see cref="object"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        internal static IEnumerable<string> AsStrings(IEnumerable<(Type Type, object? Value)> items) =>
            items.Select(item => GetText(item.Type, item.Value?.ToString()));

        /// <summary>
        /// Format the specified <paramref name="prefix"/>, <paramref name="typeName"/> and 
        /// <paramref name="values"/> according to the defined CSV format.
        /// </summary>
        /// <param name="prefix">A <see cref="string"/> that contains the prefix to put in the first column.</param>
        /// <param name="typeName">A <see cref="string"/> that contains the type name to put in the second column.</param>
        /// <param name="values">A <see cref="string[]"/> that contains the values to put in the remaining columns.</param>
        /// <returns>A <see cref="string"/> containing CSV formatted values.</returns>
        internal static string FormatCsvData(string prefix, string typeName, string[] values) => 
            $"{prefix},{typeName},{string.Join(",", values)}";

        /// <summary>
        /// Filters the specified <paramref name="lines"/> according to the specified <paramref name="prefixes"/> 
        /// and optionally rejoins split quotes if <paramref name="rejoinSplitQuotes"/> is true and 
        /// trims items if <paramref name="trimItems"/> is true.
        /// </summary>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> containing the lines to be filtered.</param>
        /// <param name="rejoinSplitQuotes">True to check for split quotes and rejoin any that are found.</param>
        /// <param name="trimItems">True to trim white space from items.</param>
        /// <param name="typeName"></param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        /// <param name="prefixes">A <see cref="string[]"/> that contains the prefixes to filter by.</param>
        internal static IEnumerable<string[]> GetItems(IEnumerable<string> lines,
                                                       bool rejoinSplitQuotes,
                                                       bool trimItems,
                                                       string? typeName, 
                                                       params string[] prefixes) =>
            lines
                .Where(line => prefixes.Any(prefix => line.StartsWith(prefix)))
                .Select(line => line.Split(','))
                .Select(items => rejoinSplitQuotes ? RejoinSplitQuotes(items) : items)
                .Select(items => trimItems ? items.Select(item => item.Trim()).ToArray() : items)
                .Where(items => prefixes.Any(prefix => items[0] == prefix))
                .Where(items => typeName is null || items[1] == typeName);

        /// <summary>
        /// Gets the names for the specified <paramref name="prefixes"/> and <paramref name="typeName"/> 
        /// from the specified <paramref name="lines"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <param name="prefixes">A <see cref="string[]"/> containing the prefixes.</param>
        /// <returns>A <see cref="string"/> array that will be null if there are no corresponding names.</returns>
        internal static string[]? GetNames(string typeName, IEnumerable<string> lines, params string[] prefixes) =>
            GetItems(lines,
                     rejoinSplitQuotes: false,
                     trimItems: true,
                     typeName,
                     prefixes)
                .Select(items => items[2..^0])
                .FirstOrDefault();

        ///// <summary>
        ///// Attempts to identify and return a <see cref="PropertyInfo"/> associated with the 
        ///// specified <paramref name="name"/> using the specified <paramref name="members"/> and 
        ///// <paramref name="propertyAndNamePairs"/>.
        ///// </summary>
        ///// <param name="members">An <see cref="OptionMember"/> array.</param>
        ///// <param name="propertyAndNamePairs">A <see cref="List{T}"/> of <see cref="PropertyAndNamePair"/> objects.</param>
        ///// <param name="name">A <see cref="string"/> containing the name.</param>
        ///// <returns>A <see cref="PropertyInfo"/> or null if none match.</returns>
        ///// <remarks>
        ///// Attempt to find the property in this order:
        ///// <list type="number">
        ///// <item>by option assignment; this overrides all others.</item>
        ///// <item>by defined name.</item>
        ///// <item>by property name.</item>
        ///// </list>
        ///// otherwise ignore it.
        ///// In all instances the name must match exactly, including case.
        ///// </remarks>
        //internal static PropertyInfo? GetProperty(OptionMember[] members,
        //                                          IEnumerable<PropertyAndNamePair> propertyAndNamePairs,
        //                                          string name) =>
        //    members
        //        .Where(member => member.Name == name)
        //        .Select(member => member.Property)
        //        .FirstOrDefault() ??
        //    propertyAndNamePairs
        //        .Where(item => item.Name == name)
        //        .Select(item => item.Property)
        //        .FirstOrDefault() ??
        //    propertyAndNamePairs
        //        .Where(item => item.Property.Name == name)
        //        .Select(item => item.Property)
        //        .FirstOrDefault();

        /// <summary>
        /// Gets the names of the data-types from the specified <paramref name="lines"/> using the 
        /// specified <paramref name="prefixes"/>.
        /// </summary>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <param name="prefixes">A <see cref="string[]"/> of prefixes that could indicate types.</param>
        /// <param name="getPrefix">A <see cref="Func{T, TResult}"/> that accepts a <see cref="string"/> containing the type name and returns <see cref="string"/> containing the prefix.</param>
        /// <returns>A <see cref="string"/> array.</returns>
        internal static string[] GetTypeNames(IEnumerable<string> lines, string[] prefixes, Func<string, string> getPrefix) =>
            lines
                // Include any lines that start with any of the prefixes.
                .Where(line => prefixes.Any(prefix => line.StartsWith(prefix)))
                .Select(line => line.Split(',', StringSplitOptions.RemoveEmptyEntries |
                                                StringSplitOptions.TrimEntries))
                .Where(items => items.Length >= 2)
                .Select(items => items[0..2])
                .Distinct()
                // Now only include items where the prefix matches that for the typename.
                .Where(items => items[0] == getPrefix(items[1]))
                .Select(items => items[1])
                .Distinct()
                .ToArray();

        /// <summary>
        /// Gets the values for the specified <paramref name="prefixes"/> and <paramref name="typeName"/> 
        /// from the specified <paramref name="lines"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <param name="prefixes">A <see cref="string[]"/> containing the prefixes.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        internal static IEnumerable<string[]> GetValues(string typeName, IEnumerable<string> lines, params string[] prefixes) =>
            GetItems(lines,
                     rejoinSplitQuotes: true,
                     trimItems: true,
                     typeName,
                     prefixes)
                .Select(items => items[2..^0]);

        /// <summary>
        /// Recombines adjacent elements where a quote delimited string has been split on a comma.
        /// </summary>
        /// <param name="elements">A <see cref="string"/> array containing the elements to check.</param>
        /// <returns>A <see cref="string"/> array.</returns>
        internal static string[] RejoinSplitQuotes(string[] elements)
        {
            // Use a list as it's easier to remove elements.
            var list = new List<string>(elements);

            // Start the index at one, check the previous element and if needed combine the
            // previous and current elements. This makes the end of bounds check easier.
            var index = 1;
            while (index < list.Count)
            {
                // We need to trim the elements before testing for leading / trailing double-quotes
                // as they still retain any leading or trailing spaces at this point. This is
                // important as there may be spaces that must be retained within the quoted text.
                // For example, "There is a comma, contained in here" would be split into two (the
                // square brackets [] are used to delimit the text for easy reading):
                //  ["There is a comma]
                //  [ contained in here"]
                // If the leading space were trimmed from the second part then they would be
                // recombined thus: "There is a comma,contained in here"; note the missing space.
                if (list[index - 1].Trim().StartsWith('"') &&
                    !list[index - 1].Trim().EndsWith('"'))
                {
                    list[index - 1] = $"{list[index - 1]},{list[index]}";
                    list.RemoveAt(index);
                }
                else
                    // Only increment the index if the original element didn't fail the check.
                    // Otherwise re-check it after it's been recombined with the following index.
                    index++;
            }

            // If the number of elements hasn't changed then return the original array.
            return elements.Length == list.Count ? elements : list.ToArray();
        }

        /// <summary>
        /// Splits the specified <paramref name="text"/> into lines on end of line characters, 
        /// removing any empty entries, trimming them and ignoring any that are comments.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text to split.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        internal static List<string> SplitLines(string text, Options options) =>
            // Split using `\r\n` CharArray rather than `Environment.NewLine` to cater for files
            // that use a differnet standard from the current OS.
            text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries |
                                             StringSplitOptions.TrimEntries)
                .Where(line => !options.CommentPrefixes.Any(prefix => line.StartsWith(prefix)))
                .ToList();

        #region Support routines

        /// <summary>
        /// Returns a <see cref="string"/> containing the textual representation of the specified
        /// <paramref name="value"/> according to the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> of the value.</param>
        /// <param name="value">A <see cref="string"/> containing the raw value.</param>
        /// <returns>A <see cref="string"/>.</returns>
        /// <remarks>
        /// <seealso cref="ConvertValue(string, string, Type)"/> does the reverse.
        /// </remarks>
        private static string GetText(Type type, string? value)
        {
            if (type == typeof(string))
                return $"\"{value ?? string.Empty}\"";

            if (type == typeof(bool))
                return value ?? false.ToString();

            if (type.IsEnum)
                // value is null or empty return empty
                // value is not defined for enum return empty
                // return name, if null return empty.
                return
                    string.IsNullOrEmpty(value) ? string.Empty :
                    !Enum.IsDefined(type, value) ? string.Empty :
                    $"{type.Name}.{value}";

            if (type.IsValueType)
                return value ?? 0.ToString();

            return string.Empty;
        }

        #endregion
    }
}
