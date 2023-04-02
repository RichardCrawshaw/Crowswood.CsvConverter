using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class ValueConversionData : BaseConversionData
    {
        private readonly List<ConversionValue> conversionValues = new();

        public ConversionValue[] ValueConversion => this.conversionValues.ToArray();

        public ValueConversionData(Deserialization.DeserializationFactory factory)
            : base(factory) { }

        public override void Deserialize()
        {
            var prefix =
                ConfigHelper.GetConversionValuePrefix(this.factory.GlobalConfig, this.factory.Options);
            var items = GetItems(prefix);
            var conversionValues = ConversionHelper.GetConversionValues(items, prefix);
            this.conversionValues.Clear();
            this.conversionValues.AddRange(conversionValues);
        }
    }
}
