using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.UserConfig;
using static Crowswood.CsvConverter.Deserialization;
using static Crowswood.CsvConverter.Handlers.ConfigHandler;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class GlobalConfigData : BaseConfigData
    {
        private readonly List<GlobalConfig> globalConfig = new();

        protected override Lazy<string[]> ValidKeys { get; } =
            new(() => ConfigHelper.GetValidConfigKeys(typeof(Converter.Keys.GlobalConfig)));

        public GlobalConfig[] GlobalConfig => this.globalConfig.ToArray();

        public GlobalConfigData(DeserializationFactory factory)
            : base(factory) { }

        public override void Deserialize()
        {
            var items =
                GetItems(typeName: null, Configurations.GlobalConfigPrefix)
                    .Where(items => this.ValidKeys.Value.Any(key => key == items[1]));
            var globalConfig = ConfigHelper.GetGlobalConfig(items);
            this.globalConfig.Clear();
            this.globalConfig.AddRange(globalConfig);
        }
    }
}
