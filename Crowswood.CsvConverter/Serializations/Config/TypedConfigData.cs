using System.Collections;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Interfaces;
using Crowswood.CsvConverter.UserConfig;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing typed config data.
    /// </summary>
    internal sealed class TypedConfigData : BaseConfigData,
        IEnumerable<ITypedConfig>
    {
        private readonly string prefix = Converter.Keys.TypedConfig.TypedConfigPrefix;
        private readonly string typeName;

        protected override Lazy<string[]> ValidKeys { get; } =
            new(() => ConfigHelper.GetValidConfigKeys(typeof(Converter.Keys.TypedConfig)));

        public TypedConfigData(string typeName, Dictionary<string, string> configuration)
            : base(configuration) => this.typeName = typeName;

        /// <inheritdoc/>
        public override string[] Serialize() =>
            GetConfiguration()
                .Select(kvp => $"{this.prefix},{this.typeName},{kvp.Key},{kvp.Value}")
                .ToArray();

        IEnumerator<ITypedConfig> IEnumerable<ITypedConfig>.GetEnumerator() => this.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        private IEnumerator<ITypedConfig> GetEnumerator()=>
            this.Configuration
                .Select(config => new TypedConfig(this.typeName, config.Key, config.Value))
                .GetEnumerator();
    }
}
