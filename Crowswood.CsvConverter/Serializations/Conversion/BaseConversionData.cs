namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstract base class for serializing conversion data.
    /// </summary>
    internal abstract class BaseConversionData : BaseSerializationData
    {
        private readonly string prefix;
        private readonly Dictionary<string, string> conversion;

        protected BaseConversionData(string prefix, Dictionary<string, string> conversion)
        {
            this.prefix = prefix;
            this.conversion = conversion;
        }

        /// <inheritdoc/>
        public override string[] Serialize() =>
            this.conversion
                .Select(kvp => $"{this.prefix},\"{kvp.Key}\",\"{kvp.Value}\"")
                .ToArray();
    }
}
