namespace Crowswood.CsvConverter.Model
{
    /// <summary>
    /// A model class to hold type name conversion data.
    /// </summary>
    internal class ConversionType
    {
        /// <summary>
        /// Gets and initialises the original value, which will be converted from.
        /// </summary>
        public string? OriginalTypeName { get; init; }

        /// <summary>
        /// Gets and initialises the converted value.
        /// </summary>
        public string? ConvertedTypeName { get; init; }
    }
}
