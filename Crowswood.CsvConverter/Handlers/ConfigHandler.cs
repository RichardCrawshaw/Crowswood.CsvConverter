using Crowswood.CsvConverter.Extensions;
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
        /// Create a default instance.
        /// </summary>
        public ConfigHandler() 
            : this(Options.None, Array.Empty<GlobalConfig>(), Array.Empty<TypedConfig>())
        { }

        /// <summary>
        /// Create an instance with configuration values from the specified <paramref name="lines"/>.
        /// </summary>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> that contains the data from which the configuration information is to be extracted.</param>
        public ConfigHandler(Options options, IEnumerable<string> lines)
            : this(options, ConversionHelper.GetItems(lines,
                                                      rejoinSplitQuotes: true,
                                                      trimItems: true,
                                                      typeName: null,
                                                      Configurations.GlobalConfigPrefix,
                                                      Configurations.TypedConfigPrefix,
                                                      string.Empty))
        { }

        /// <summary>
        /// Private constructor to create an instance with configuraiton values from the 
        /// specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that contains the items from which the configuration information is to be extracted.</param>
        private ConfigHandler(Options options, IEnumerable<string[]> items)
            : this(options, GetGlobalConfig(items), GetTypedConfig(items)) { }

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
        /// Gets the <see cref="GlobalConfig"/> item that has the specified <paramref name="name"/>, 
        /// or null if there is no match.
        /// </summary>
        /// <param name="name">A <see cref="string"/> that contains the name of a configuration item.</param>
        /// <returns>A <see cref="GlobalConfig"/> or null.</returns>
        public GlobalConfig? GetGlobal(string name) =>
            this.globalConfig
                .FirstOrDefault(config => config.Name == name);

        /// <summary>
        /// Gets all the <see cref="TypedConfig"/> items that have the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="name">A <see cref="string"/> that contains the name of a configuration item.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="GlobalConfig"/>.</returns>
        public IEnumerable<TypedConfig> GetTyped(string name) =>
            this.typedConfig
                .Where(config => config.Name == name);

        /// <summary>
        /// Gets the <see cref="TypedConfig"/> item that has the specified <paramref name="typeName"/> 
        /// and <paramref name="name"/>, or null if there is no match.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the name of a data-type.</param>
        /// <param name="name">A <see cref="string"/> that contains the name of a configuration item.</param>
        /// <returns>A <see cref="TypedConfig"/> or null.</returns>
        public TypedConfig? GetTyped(string typeName, string name)=>
            this.typedConfig
                .FirstOrDefault(config=> config.TypeName == typeName && config.Name == name);

        /// <summary>
        /// Gets the property prefix for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <returns>A <see cref="string"/> containing the prefix.</returns>
        public string GetPropertyPrefix(string typeName) =>
            GetTyped(typeName, Configurations.PropertyPrefix)?.Value ??
            GetGlobal(Configurations.PropertyPrefix)?.Value ??
            this.options.PropertyPrefix; // GetPropertyPrefix

        /// <summary>
        /// Gets all the property prefixes.
        /// </summary>
        /// <returns>A <see cref="string[]"/> containing the prefixes.</returns>
        public string[] GetPropertyPrefixes() =>
            new List<string?>(
                GetTyped(Configurations.PropertyPrefix)
                    .Select(config => config.Value))
            {
                GetGlobal(Configurations.PropertyPrefix)?.Value,
                this.options.PropertyPrefix, // GetPropertyPrefixes

            }.NotNull()
            .Distinct()
            .ToArray();

        /// <summary>
        /// Gets the name of the id column for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the name of the data type being referenced.</param>
        /// <returns>A <see cref="string"/> containing the name of the id column.</returns>
        public string GetReferenceIdColumnName(string typeName) =>
            GetTyped(typeName, Configurations.ReferenceIdColumnName)?.Value ??
            GetGlobal(Configurations.ReferenceIdColumnName)?.Value ??
            GetOptionReference(typeName)?.IdPropertyName ??
            GetOptionReference()?.IdPropertyName ??
            Configurations.DefaultIdColumnName;

        /// <summary>
        /// Gets the name of the name column for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the name of the data type being referenced.</param>
        /// <returns>A <see cref="string"/> containing the name of the name column.</returns>
        public string GetReferenceNameColumnName(string typeName) =>
            GetTyped(typeName, Configurations.ReferenceNameColumnName)?.Value ??
            GetGlobal(Configurations.ReferenceNameColumnName)?.Value ??
            GetOptionReference(typeName)?.NamePropertyName ??
            GetOptionReference()?.NamePropertyName ??
            Configurations.DefaultNameColumnName;

        /// <summary>
        /// Gets the value prefix for the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <returns>A <see cref="string"/> containing the prefix.</returns>
        public string GetValuePrefix(string typeName) =>
            GetTyped(typeName, Configurations.ValuesPrefix)?.Value ??
            GetGlobal(Configurations.ValuesPrefix)?.Value ??
            this.options.ValuesPrefix;

        /// <summary>
        /// Gets all the value prefixes.
        /// </summary>
        /// <returns>A <see cref="string[]"/> containing the prefixes.</returns>
        public string[] GetValuePrefixes() =>
            new List<string?>(
                GetTyped(Configurations.ValuesPrefix)
                    .Select(config => config.Value))
            {
                GetGlobal(Configurations.ValuesPrefix)?.Value,
                this.options.ValuesPrefix,
            }.NotNull()
            .ToArray();

        #endregion

        #region Support routines

        /// <summary>
        /// Wrapper routine that gets the <see cref="OptionReference"/> for the default.
        /// </summary>
        /// <returns>An <see cref="OptionReference"/>; null if there is no default defined.</returns>
        private OptionReference? GetOptionReference() =>
            this.options.OptionsReferences.Get();

        /// <summary>
        /// Wrapper routine that gets the <see cref="OptionReferenceType"/> for the specified 
        /// <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <returns>An <see cref="OptionReferenceType"/>; null if there is nothing defined for the <paramref name="typeName"/>.</returns>
        private OptionReferenceType? GetOptionReference(string typeName) =>
            this.options.OptionsReferences.Get(typeName);

        /// <summary>
        /// Gets all the <see cref="GlobalConfig"/> items from the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that contains the items from which the global configuration information is to be extracted.</param>
        /// <returns>An <see cref="GlobalConfig"/> array.</returns>
        /// <exception cref="Exception">If there are duplicate definitions; each name must be unique.</exception>
        private static GlobalConfig[] GetGlobalConfig(IEnumerable<string[]> items)
        {
            var globalConfig =
                items
                    .Where(items => items[0] == Configurations.GlobalConfigPrefix)
                    .Where(items => items.Length >= 3)
                    .Select(items => new { Name = items[1], Value = items[2], })
                    .Select(n => new GlobalConfig(n.Name, n.Value))
                    .ToArray();
            // Each Name must be unique.
            if (globalConfig.Any(n => globalConfig.Count(m => m.Name == n.Name) > 1))
                throw new Exception("Duplicate Global Config definitions.");
            return globalConfig;
        }

        /// <summary>
        /// Gets all the <see cref="TypedConfig"/> items from the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that contains the items from which the typed configuration information is to be extracted.</param>
        /// <returns>A <see cref="TypedConfig"/> array.</returns>
        /// <exception cref="Exception">If there are duplicate definitions; each typename and name conbination must be unique.</exception>
        private static TypedConfig[] GetTypedConfig(IEnumerable<string[]> items)
        {
            if (items.Any())
            {
                Exception? ex = null;
                if (!items.Any(n => n[0] == Configurations.TypedConfigPrefix))
                {
                    ex = new ArgumentException("No Typed config.");
                    ex.Data["TypedConfig"] =
                        items
                            .Select(n => n[0])
                            .ToList();
                }

                if (!items.Any(n => n.Length >= 4))
                {
                    ex = new ArgumentException("Insufficient number of items for valid Typed config.");
                    ex.Data["Length"] =
                        items
                            .Where(n => n[0] == Configurations.TypedConfigPrefix)
                            .Select(n => n.Length.ToString())
                            .ToList();
                }

                if (ex != null)
                    throw ex;
            }

            var typedConfig =
                items
                    .Where(items => items[0] == Configurations.TypedConfigPrefix)
                    .Where(items => items.Length >= 4)
                    .Select(items => new { TypeName = items[1], Name = items[2], Value = items[3], })
                    .Select(n => new TypedConfig(n.TypeName, n.Name, n.Value))
                    .ToArray();
            // Each combinatin of TypeName and Name must be unique.
            if (typedConfig.Any(n => typedConfig.Count(m => m.TypeName == n.TypeName && m.Name == n.Name) > 1))
                throw new Exception("Duplicate Typed Config definitions.");
            return typedConfig;
        }

        #endregion
    }
}
