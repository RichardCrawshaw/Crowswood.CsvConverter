using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.UserConfig;

namespace Crowswood.CsvConverter.Handlers
{
    /// <summary>
    /// An internal class that is to manage the user supplied configuration.
    /// </summary>
    internal sealed class ConfigHandler
    {
        #region Fields

        /// <summary>
        /// An internal static class that contains constant strings used within the 
        /// <see cref="ConfigHandler"/>.
        /// </summary>
        internal static class Configurations
        {
            public const string GlobalConfigPrefix = "GlobalConfig";
            public const string TypedConfigPrefix = "TypedConfig";

            public const string PropertyPrefix = "PropertyPrefix";
            public const string ValuesPrefix = "ValuesPrefix";

            public const string DefaultIdColumnName = "Id";
            public const string DefaultNameColumnName = "Name";

            public const string ReferenceIdColumnName = "ReferenceIdColumnName";
            public const string ReferenceNameColumnName = "ReferenceNameColumnName";

            public const string ConversionTypePrefix = "ConversionTypePrefix";
            public const string ConversionValuePrefix = "ConversionValuePrefix";
        }

        private readonly Options options;

        private readonly List<GlobalConfig> globalConfig = new();
        private readonly List<TypedConfig> typedConfig = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="GlobalConfig"/> managed by the current instance.
        /// </summary>
        public GlobalConfig[] GlobalConfig => this.globalConfig.ToArray();

        /// <summary>
        /// Gets the <see cref="TypedConfig"/> managed by the current instance.
        /// </summary>
        public TypedConfig[] TypedConfig => this.typedConfig.ToArray();

        #endregion

        #region Constructors

        /// <summary>
        /// Create an instance with the specified <paramref name="options"/>.
        /// </summary>
        internal ConfigHandler(Options options)
            : this(options, new List<string>())
        { }

        /// <summary>
        /// Create an instance with configuration values from the specified <paramref name="lines"/>.
        /// </summary>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> that contains the data from which the configuration information is to be extracted.</param>
        internal ConfigHandler(Options options, IEnumerable<string> lines)
            : this(options, ConverterHelper.GetItems(lines,
                                                      rejoinSplitQuotes: true,
                                                      trimItems: true,
                                                      typeName: null,
                                                      Configurations.GlobalConfigPrefix,
                                                      Configurations.TypedConfigPrefix))
        { }

        /// <summary>
        /// Private constructor to create an instance with configuraiton values from the 
        /// specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that contains the items from which the configuration information is to be extracted.</param>
        private ConfigHandler(Options options, IEnumerable<string[]> items)
            : this(options, ConfigHelper.GetGlobalConfig(items), ConfigHelper.GetTypedConfig(items)) { }

        /// <summary>
        /// Private constructor to create an instance using the specified <paramref name="globalConfig"/> 
        /// and <paramref name="typedConfig"/>.
        /// </summary>
        /// <param name="globalConfig">A <see cref="GlobalConfig"/> array.</param>
        /// <param name="typedConfig">A <see cref="TypedConfig"/> array.</param>
        private ConfigHandler(Options options, GlobalConfig[] globalConfig, TypedConfig[] typedConfig)
        {
            this.options = options;
            this.globalConfig.AddRange(globalConfig);
            this.typedConfig.AddRange(typedConfig);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the conversion type prefix.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the prefix.</returns>
        internal string GetConversionTypePrefix() =>
            ConfigHelper.GetConversionTypePrefix(this.globalConfig, this.options);

        /// <summary>
        /// Gets the conversion value prefix.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the prefix.</returns>
        internal string GetConversionValuePrefix() =>
            ConfigHelper.GetConversionValuePrefix(this.globalConfig, this.options);

        /// <summary>
        /// Gets the property prefix for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <returns>A <see cref="string"/> containing the prefix.</returns>
        public string GetPropertyPrefix(string typeName) =>
            ConfigHelper.GetPropertyPrefix(this.globalConfig, this.typedConfig, this.options, typeName);

        /// <summary>
        /// Gets all the property prefixes.
        /// </summary>
        /// <returns>A <see cref="string[]"/> containing the prefixes.</returns>
        public string[] GetPropertyPrefixes() =>
            ConfigHelper.GetPropertyPrefixes(this.globalConfig, this.typedConfig, this.options);

        /// <summary>
        /// Gets the name of the id column for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the name of the data type being referenced.</param>
        /// <returns>A <see cref="string"/> containing the name of the id column.</returns>
        public string GetReferenceIdColumnName(string typeName) =>
            ConfigHelper.GetReferenceIdColumnName(this.globalConfig, this.typedConfig, this.options, typeName);

        /// <summary>
        /// Gets the name of the name column for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the name of the data type being referenced.</param>
        /// <returns>A <see cref="string"/> containing the name of the name column.</returns>
        public string GetReferenceNameColumnName(string typeName) =>
            ConfigHelper.GetReferenceNameColumnName(this.globalConfig, this.typedConfig, this.options, typeName);

        /// <summary>
        /// Gets the value prefix for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <returns>A <see cref="string"/> containing the prefix.</returns>
        public string GetValuePrefix(string typeName) =>
            ConfigHelper.GetValuePrefix(this.globalConfig, this.typedConfig, this.options, typeName);

        /// <summary>
        /// Gets all the value prefixes.
        /// </summary>
        /// <returns>A <see cref="string[]"/> containing the prefixes.</returns>
        public string[] GetValuePrefixes() =>
            ConfigHelper.GetValuePrefixes(this.globalConfig, this.typedConfig, this.options);

        #endregion
    }
}
