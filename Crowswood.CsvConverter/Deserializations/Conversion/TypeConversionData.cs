using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class TypeConversionData : BaseConversionData
    {
        private readonly List<ConversionType> conversionTypes = new();

        public ConversionType[] TypedConversion => this.conversionTypes.ToArray();

        public TypeConversionData(DeserializationFactory factory)
            : base(factory) { }

        public override void Deserialize()
        {
            var prefix =
                ConfigHelper.GetConversionTypePrefix(this.factory.GlobalConfig, this.factory.Options);
            var items = GetItems(prefix);
            var conversionTypes = ConversionHelper.GetConversionTypes(items, prefix);
            this.conversionTypes.Clear();
            this.conversionTypes.AddRange(conversionTypes);
        }
    }
}
