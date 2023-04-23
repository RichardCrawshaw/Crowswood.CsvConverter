using Crowswood.CsvConverter.Helpers;

namespace Crowswood.CsvConverter.Extensions
{
    public static class ConverterExtensions
    {
        /// <summary>
        /// Constructs from the <paramref name="values"/> and the specified <paramref name="prefixes"/>
        /// a single comma separated <see cref="string"/>.
        /// </summary>
        /// <param name="values">A <see cref="string[]"/> containing the values.</param>
        /// <param name="prefixes">A params <see cref="string[]"/> containing any prefixes to include.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string AsCsv(this string[] values, params string[] prefixes) =>
            string.Join(",",
                new List<string[]> { prefixes, values, }
                    .SelectMany(items => items));

        /// <summary>
        /// Deserialize from the <paramref name="stream"/> returning an <see cref="IEnumerable{T}"/> 
        /// of <typeparamref name="TBase"/> objects.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to return.</typeparam>
        /// <param name="converter">The <see cref="Converter"/> to use.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the data to deserialize.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.</returns>
        public static IEnumerable<TBase> Deserialize<TBase>(this Converter converter, Stream stream)
            where TBase : class
        {
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var result = converter.Deserialize<TBase>(text);
            return result;
        }

        /// <summary>
        /// Deserialize from the specified <paramref name="stream"/> returning a typeless data 
        /// object.
        /// </summary>
        /// <param name="converter">The <see cref="Converter"/> to use.</param>
        /// <param name="stream">A <see cref="Stream"/> that contains the data to deserialize.</param>
        /// <returns>A <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> keyed by <see cref="string"/>.</returns>
        public static Dictionary<string, (string[], IEnumerable<string[]>)> Deserialize(this Converter converter, Stream stream)
        {
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var result = converter.Deserialize(text);
            return result;
        }

        /// <summary>
        /// Gets the items from the <paramref name="lines"/> that correspond to the specified 
        /// <paramref name="typeName"/> and any of the specified <paramref name="prefixes"/>.
        /// </summary>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <param name="prefixes">A <see cref="string[]"/> containing the prefixes.</param>
        /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        public static IEnumerable<string[]> GetItems(this IEnumerable<string> lines, string? typeName, params string[] prefixes)
        {
            var filteredLines =
                lines
                    .Where(line => prefixes.Any(prefix => line.StartsWith(prefix)));
            var items =
                ConverterHelper.GetItems(lines,
                                         rejoinSplitQuotes: true,
                                         trimItems: true,
                                         typeName: typeName,
                                         prefixes: prefixes);
            return items;
        }

        /// <summary>
        /// Gets the property names from the specified <paramref name="items"/> identified by the 
        /// specified <paramref name="prefix"/> and <paramref name="objectTypeName"/> removing the 
        /// leading two elements.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containging the items.</param>
        /// <param name="prefix">A <see cref="string"/> containing the prefix that identifies the properties.</param>
        /// <param name="dataTypeName">A <see cref="string"/> containing the name of the data type.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        /// <exception cref="InvalidOperationException">If the <paramref name="items"/> does not contain any element that corresponds to the <paramref name="propertiesPrefix"/> and the data type name.</exception>
        public static string[] GetPropertyNames(this IEnumerable<string[]> items, string prefix, string dataTypeName) =>
            items
                .Where(items => items[0] == prefix)
                .Where(items => items[1] == dataTypeName)
                .Select(items => items[2..])
                .FirstOrDefault() ??
            throw new InvalidOperationException(
                $"No property names found for '{dataTypeName}'.");

        /// <summary>
        /// Gets the values from the specified <paramref name="items"/> identified by the 
        /// specified <paramref name="prefix"/> and <paramref name="dataTypeName"/> removing the 
        /// leading two elements.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containging the items.</param>
        /// <param name="prefix">A <see cref="string"/> containing the prefix that identifies the values.</param>
        /// <param name="dataTypeName">A <see cref="string"/> containing the name of the data type.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        public static IEnumerable<string[]> GetValues(this IEnumerable<string[]> items, string prefix, string dataTypeName) =>
            items
                .Where(items => items[0] == prefix)
                .Where(items => items[1] == dataTypeName)
                .Select(items => items[2..])
                .ToArray();
    }
}
