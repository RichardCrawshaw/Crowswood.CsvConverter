using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class TypeConversionData : BaseConversionData<ConversionType>
    {
        /// <summary>
        /// Gets the type conversion data.
        /// </summary>
        public ConversionType[] TypedConversion => this.Conversions;

        public TypeConversionData(DeserializationFactory factory)
            : base(factory) { }

        /// <inheritdoc/>
        protected override ConversionType[] GetConversions(IEnumerable<string[]> items, string prefix) =>
            ConversionHelper.GetConversionTypes(items, prefix);

        /// <inheritdoc/>
        protected override string GetPrefix() =>
            ConfigHelper.GetConversionTypePrefix(this.factory.GlobalConfig, this.factory.Options);
    }
}
