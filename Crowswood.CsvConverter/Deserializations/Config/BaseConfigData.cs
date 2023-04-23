using Crowswood.CsvConverter.UserConfig;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal abstract class BaseConfigData : BaseDeserializationData
    {
        /// <summary>
        /// Gets the keys that are valid for the Configuration.
        /// </summary>
        protected abstract Lazy<string[]> ValidKeys { get; }

        protected BaseConfigData(DeserializationFactory factory)
            : base(factory) { }
    }

    internal abstract class BaseConfigData<TConfig> : BaseConfigData
        where TConfig : BaseConfig
    {
        private readonly List<TConfig> configData = new();
        private readonly string prefix;
        private readonly int index;

        /// <summary>
        /// Gets the config data.
        /// </summary>
        public TConfig[] ConfigData => this.configData.ToArray();

        protected BaseConfigData(DeserializationFactory factory, string prefix, int index)
            : base(factory)
        {
            this.prefix = prefix;
            this.index = index;
        }

        /// <summary>
        /// Gets the items that contain config data.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        protected IEnumerable<string[]> GetItems() =>
            GetItems(typeName: null, this.prefix)
                .Where(items => this.ValidKeys.Value.Any(key => key == items[index]));

        /// <summary>
        /// Gets the config data from the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="[]"/> that contains the config data.</param>
        /// <returns>A <typeparamref name="TConfig"/> array.</returns>
        protected abstract TConfig[] GetConfig(IEnumerable<string[]> items);

        /// <inheritdoc/>
        public override void Deserialize()
        {
            var items = GetItems();
            var config = GetConfig(items);
            this.configData.Clear();
            this.configData.AddRange(config);
        }
    }
}
