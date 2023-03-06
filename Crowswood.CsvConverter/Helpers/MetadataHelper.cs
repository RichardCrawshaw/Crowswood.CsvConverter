namespace Crowswood.CsvConverter.Helpers
{
    /// <summary>
    /// Static helper class to help with metadata.
    /// </summary>
    internal static class MetadataHelper
    {

        /// <summary>
        /// Creates and returns a dictionary of values using the specified <paramref name="valueConverter"/>
        /// for the specified <paramref name="names"/> and <paramref name="values"/> and 
        /// <paramref name="allowNulls"/> flag.
        /// </summary>
        /// <param name="valueConverter">The <see cref="ValueConverter"/> to use to convert the values.</param>
        /// <param name="names">A <see cref="string"/> array containing the names that will form the keys.</param>
        /// <param name="values">A <see cref="string"/> array containing the values.</param>
        /// <param name="allowNulls">A <see cref="bool"/> that if true indicates that an empty string will generate null; false to generate an empty string. An string containing empty double-quotes will always generate an empty string.</param>
        /// <returns></returns>
        public static Dictionary<string, string?> GetMetadataDictionary(ValueConverter valueConverter,
                                                                         string[] names,
                                                                         string[] values,
                                                                         bool allowNulls) =>
            Enumerable
                .Range(0, Math.Min(names.Length, values.Length))
                .ToDictionary(
                    index => names[index],
                    index => values[index] switch
                    {
                        "" => allowNulls ? null : string.Empty,
                        "\"\"" => string.Empty,
                        _ => valueConverter.ConvertValue(values[index], typeof(string))?.ToString() ??
                            string.Empty,
                    });
    }
}