using System.Collections;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Interfaces;
using Crowswood.CsvConverter.UserConfig;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing global config data.
    /// </summary>
    internal sealed class GlobalConfigData : BaseConfigData,
        IEnumerable<IGlobalConfig>
    {
        private readonly string prefix = Converter.Keys.GlobalConfig.GlobalConfigPrefix;

        protected override Lazy<string[]> ValidKeys { get; } =
            new(() => ConfigHelper.GetValidConfigKeys(typeof(Converter.Keys.GlobalConfig)));

        public GlobalConfigData(Dictionary<string, string> configuration)
            : base(configuration) { }

        /// <inheritdoc/>
        public override string[] Serialize() =>
            GetConfiguration()
                .Select(kvp => $"{this.prefix},{kvp.Key},{kvp.Value}")
                .ToArray();

        IEnumerator<IGlobalConfig> IEnumerable<IGlobalConfig>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerator<IGlobalConfig> GetEnumerator() =>
            this.Configuration
                .Select(config => new GlobalConfig(config.Key, config.Value))
                .GetEnumerator();
    }
}
