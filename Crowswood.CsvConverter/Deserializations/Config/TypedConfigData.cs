using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.UserConfig;
using static Crowswood.CsvConverter.Deserialization;
using static Crowswood.CsvConverter.Handlers.ConfigHandler;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class TypedConfigData : BaseConfigData<TypedConfig>
    {
        protected sealed override Lazy<string[]> ValidKeys { get; } =
            new(() => ConfigHelper.GetValidConfigKeys(typeof(Converter.Keys.TypedConfig)));

        public TypedConfig[] TypedConfig => this.ConfigData;

        public TypedConfigData(DeserializationFactory factory)
            : base(factory, Configurations.TypedConfigPrefix, index: 2) { }

        protected sealed override TypedConfig[] GetConfig(IEnumerable<string[]> items) =>
            ConfigHelper.GetTypedConfig(items);
    }
}
