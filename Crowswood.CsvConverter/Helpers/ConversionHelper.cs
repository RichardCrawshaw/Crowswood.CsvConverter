using System.Reflection;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Helpers
{
    internal static class ConversionHelper
    {
        /// <summary>
        /// Converts and returns the values of the properties of the specified <paramref name="item"/> 
        /// that are in the <paramref name="properties"/> into an <see cref="IEnumerable{T}"/> of 
        /// <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of item to process.</typeparam>
        /// <param name="item">A <typeparamref name="TBase"/> object.</param>
        /// <param name="properties">A <see cref="PropertyInfo"/> array.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        internal static IEnumerable<string> AsStrings<TBase>(TBase item, PropertyInfo[] properties)
            where TBase : class
        {
            var results = new List<string>();

            foreach (var property in properties)
            {
                var value = property.GetValue(item)?.ToString();
                var text = GetText(property.PropertyType, value);
                results.Add(text);
            }

            return results;
        }

        /// <summary>
        /// Attempts to identify and return a <see cref="PropertyInfo"/> associated with the 
        /// specified <paramref name="name"/> using the specified <paramref name="members"/> and 
        /// <paramref name="propertyAndNamePairs"/>.
        /// </summary>
        /// <param name="members">An <see cref="OptionMember"/> array.</param>
        /// <param name="propertyAndNamePairs">A <see cref="List{T}"/> of <see cref="PropertyAndNamePair"/> objects.</param>
        /// <param name="name">A <see cref="string"/> containing the name.</param>
        /// <returns>A <see cref="PropertyInfo"/> or null if none match.</returns>
        /// <remarks>
        /// Attempt to find the property in this order:
        /// <list type="number">
        /// <item>by option assignment; this overrides all others.</item>
        /// <item>by defined name.</item>
        /// <item>by property name.</item>
        /// </list>
        /// otherwise ignore it.
        /// In all instances the name must match exactly, including case.
        /// </remarks>
        internal static PropertyInfo? GetProperty(OptionMember[] members,
                                                  IEnumerable<PropertyAndNamePair> propertyAndNamePairs,
                                                  string name) =>
            members
                .Where(member => member.Name == name)
                .Select(member => member.Property)
                .FirstOrDefault() ??
            propertyAndNamePairs
                .Where(item => item.Name == name)
                .Select(item => item.Property)
                .FirstOrDefault() ??
            propertyAndNamePairs
                .Where(item => item.Property.Name == name)
                .Select(item => item.Property)
                .FirstOrDefault();

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
