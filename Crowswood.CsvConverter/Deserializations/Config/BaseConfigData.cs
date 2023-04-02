using Crowswood.CsvConverter.Helpers;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal abstract class BaseConfigData : BaseDeserializationData
    {
        protected abstract Lazy<string[]> ValidKeys { get; }

        protected BaseConfigData(DeserializationFactory factory)
            : base(factory) { }
    }
}
