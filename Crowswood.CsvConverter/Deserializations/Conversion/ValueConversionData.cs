using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class ValueConversionData : BaseConversionData<ConversionValue>
    {
        /// <summary>
        /// Gets the value conversion data.
        /// </summary>
        public ConversionValue[] ValueConversion => this.Conversions;

        public ValueConversionData(Deserialization.DeserializationFactory factory)
            : base(factory) { }

        /// <inheritdoc/>
        protected override ConversionValue[] GetConversions(IEnumerable<string[]> items, string prefix) =>
            ConversionHelper.GetConversionValues(items, prefix);

        /// <inheritdoc/>
        protected override string GetPrefix() =>
            ConfigHelper.GetConversionValuePrefix(this.factory.GlobalConfig, this.factory.Options);
    }
}
