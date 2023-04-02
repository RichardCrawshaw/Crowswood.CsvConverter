using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.UserConfig;
using static Crowswood.CsvConverter.Deserialization;
using static Crowswood.CsvConverter.Handlers.ConfigHandler;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class TypedConfigData : BaseConfigData
    {
        private readonly List<TypedConfig> typedConfig = new();

        protected override Lazy<string[]> ValidKeys { get; } =
            new(() => ConfigHelper.GetValidConfigKeys(typeof(Converter.Keys.TypedConfig)));

        public TypedConfig[] TypedConfig => typedConfig.ToArray();

        public TypedConfigData(DeserializationFactory factory)
            : base(factory) { }

        public override void Deserialize()
        {
            var items =
                GetItems(typeName: null, Configurations.TypedConfigPrefix)
                    .Where(items => this.ValidKeys.Value.Any(key => key == items[2]));
            var globalConfig = ConfigHelper.GetTypedConfig(items);
            this.typedConfig.Clear();
            this.typedConfig.AddRange(globalConfig);
        }
    }
}
