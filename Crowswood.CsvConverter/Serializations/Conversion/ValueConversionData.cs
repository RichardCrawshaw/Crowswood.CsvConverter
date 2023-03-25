using Crowswood.CsvConverter.Handlers;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing value conversion data.
    /// </summary>
    internal sealed class ValueConversionData : BaseConversionData
    {
        public ValueConversionData(Dictionary<string, string> valueConversion)
            : base(ConfigHandler.Configurations.ConversionValuePrefix, valueConversion) { }
    }
}
