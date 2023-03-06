namespace Crowswood.CsvConverter.Model
{
    /// <summary>
    /// A model class to hold value conversion data.
    /// </summary>
    internal class ConversionValue
    {
        /// <summary>
        /// Gets and initialises the original value, which will be converted from.
        /// </summary>
        public string? OriginalValue { get; init; }

        /// <summary>
        /// Gets and initialises the converted value.
        /// </summary>
        public string? ConvertedValue { get; init; }
    }
}
