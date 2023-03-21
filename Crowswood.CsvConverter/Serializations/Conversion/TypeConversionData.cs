using Crowswood.CsvConverter.Handlers;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing type conversion data.
    /// </summary>
    internal sealed class TypeConversionData : BaseConversionData
    {
        public TypeConversionData(Dictionary<string, string> typeConversion)
            : base(ConfigHandler.Configurations.ConversionTypePrefix, typeConversion) { }
    }
}
