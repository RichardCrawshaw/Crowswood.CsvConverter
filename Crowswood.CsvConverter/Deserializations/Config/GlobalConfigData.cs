using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.UserConfig;
using static Crowswood.CsvConverter.Deserialization;
using static Crowswood.CsvConverter.Handlers.ConfigHandler;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class GlobalConfigData : BaseConfigData<GlobalConfig>
    {
        protected sealed override Lazy<string[]> ValidKeys { get; } =
            new(() => ConfigHelper.GetValidConfigKeys(typeof(Converter.Keys.GlobalConfig)));

        public GlobalConfig[] GlobalConfig => this.ConfigData;

        public GlobalConfigData(DeserializationFactory factory)
            : base(factory, Configurations.GlobalConfigPrefix, index: 1) { }

        protected sealed override GlobalConfig[] GetConfig(IEnumerable<string[]> items) => 
            ConfigHelper.GetGlobalConfig(items);
    }
}
