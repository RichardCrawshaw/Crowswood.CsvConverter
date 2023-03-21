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
    }
}
