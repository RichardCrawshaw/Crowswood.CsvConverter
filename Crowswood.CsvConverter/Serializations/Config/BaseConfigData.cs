namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstract base class for serializing config data.
    /// </summary>
    internal abstract class BaseConfigData : BaseSerializationData
    {
        protected Dictionary<string, string> Configuration { get; }

        protected abstract Lazy<string[]> ValidKeys { get; }

        protected BaseConfigData(Dictionary<string, string> configuration) =>
            this.Configuration = configuration;

        protected IEnumerable<KeyValuePair<string, string>> GetConfiguration() =>
            this.Configuration
                .Where(kvp => this.ValidKeys.Value.Any(key => key == kvp.Key));
    }
}
