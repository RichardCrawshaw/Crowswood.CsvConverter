using System.Reflection;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Interfaces;
using Crowswood.CsvConverter.UserConfig;
using static Crowswood.CsvConverter.Handlers.ConfigHandler;

namespace Crowswood.CsvConverter.Helpers
{
    /// <summary>
    /// Static helper class to help with <see cref="UserConfig.GlobalConfig"/> and 
    /// <see cref="UserConfig.TypedConfig"/>.
    /// </summary>
    internal static class ConfigHelper
    {
        /// <summary>
        /// Gets all the <see cref="GlobalConfig"/> items from the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that contains the items from which the global configuration information is to be extracted.</param>
        /// <returns>An <see cref="GlobalConfig"/> array.</returns>
        /// <exception cref="Exception">If there are duplicate definitions; each name must be unique.</exception>
        public static GlobalConfig[] GetGlobalConfig(IEnumerable<string[]> items)
        {
            var globalConfig =
                items
                    .Where(items => items[0] == ConfigHandler.Configurations.GlobalConfigPrefix)
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
        public static TypedConfig[] GetTypedConfig(IEnumerable<string[]> items)
        {
            var typedConfig =
                items
                    .Where(items => items[0] == ConfigHandler.Configurations.TypedConfigPrefix)
                    .Where(items => items.Length >= 4)
                    .Select(items => new { TypeName = items[1], Name = items[2], Value = items[3], })
                    .Select(n => new TypedConfig(n.TypeName, n.Name, n.Value))
                    .ToArray();
            // Each combinatin of TypeName and Name must be unique.
            if (typedConfig.Any(n => typedConfig.Count(m => m.TypeName == n.TypeName && m.Name == n.Name) > 1))
                throw new Exception("Duplicate Typed Config definitions.");
            return typedConfig;
        }

        /// <summary>
        /// Gets the value of the static public fields of the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        public static string[] GetValidConfigKeys(Type type) =>
            type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(string))
                .Select(field => field.GetValue(null))
                .Cast<string>()
                .ToArray();

        /// <summary>
        /// Gets the Conversion Type Prefix from the specified <paramref name="globalConfig"/> and 
        /// <paramref name="options"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <returns>A <see cref="string"/></returns>
        public static string GetConversionTypePrefix(IEnumerable<IGlobalConfig> globalConfig, Options options) =>
            globalConfig.GetGlobal(Configurations.ConversionTypePrefix)?.Value ??
            options.ConversionTypePrefix;

        /// <summary>
        /// Gets the Conversion Value Prefix from the specified <paramref name="globalConfig"/> and 
        /// <paramref name="options"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <returns>A <see cref="string"/></returns>
        public static string GetConversionValuePrefix(IEnumerable<IGlobalConfig> globalConfig, Options options) =>
            globalConfig.GetGlobal(Configurations.ConversionValuePrefix)?.Value ??
            options.ConversionValuePrefix;

        /// <summary>
        /// Gets the Property Prefix from the specified <paramref name="globalConfig"/>, 
        /// <paramref name="typedConfig"/> and <paramref name="options"/> for the specified 
        /// <paramref name="typeName"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="typedConfig">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <param name="typeName">A <see cref="string"/> containing the type-name.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetPropertyPrefix(IEnumerable<IGlobalConfig> globalConfig, IEnumerable<ITypedConfig> typedConfig, Options options, string typeName) =>
            typedConfig.GetTyped(typeName, Configurations.PropertyPrefix)?.Value ??
            globalConfig.GetGlobal(Configurations.PropertyPrefix)?.Value ??
            options.PropertyPrefix;

        /// <summary>
        /// Gets all the Property Prefixes from the specified <paramref name="globalConfig"/>, 
        /// <paramref name="typedConfig"/> and <paramref name="options"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="typedConfig">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        public static string[] GetPropertyPrefixes(IEnumerable<IGlobalConfig> globalConfig, IEnumerable<ITypedConfig> typedConfig, Options options) =>
            GetAllItems(globalConfig, typedConfig, options.PropertyPrefix, Configurations.PropertyPrefix);

        /// <summary>
        /// Gets the ReferenceId Column Name from the specified <paramref name="globalConfig"/>, 
        /// <paramref name="typedConfig"/> and <paramref name="options"/> for the specified 
        /// <paramref name="typeName"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="typedConfig">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <param name="typeName">A <see cref="string"/> containing the type-name.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetReferenceIdColumnName(IEnumerable<IGlobalConfig> globalConfig, IEnumerable<ITypedConfig> typedConfig, Options options, string typeName) =>
            typedConfig.GetTyped(typeName, Configurations.ReferenceIdColumnName)?.Value ??
            globalConfig.GetGlobal(Configurations.ReferenceIdColumnName)?.Value ??
            options.OptionsReferences.Get(typeName)?.IdPropertyName ??
            options.OptionsReferences.Get()?.IdPropertyName ??
            Configurations.DefaultIdColumnName;

        /// <summary>
        /// Gets the ReferenceName Column Name from the specified <paramref name="globalConfig"/>, 
        /// <paramref name="typedConfig"/> and <paramref name="options"/> for the specified 
        /// <paramref name="typeName"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="typedConfig">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <param name="typeName">A <see cref="string"/> containing the type-name.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetReferenceNameColumnName(IEnumerable<IGlobalConfig> globalConfig, IEnumerable<ITypedConfig> typedConfig, Options options, string typeName) =>
            typedConfig.GetTyped(typeName, Configurations.ReferenceNameColumnName)?.Value ??
            globalConfig.GetGlobal(Configurations.ReferenceNameColumnName)?.Value ??
            options.OptionsReferences.Get(typeName)?.NamePropertyName ??
            options.OptionsReferences.Get()?.NamePropertyName ??
            Configurations.DefaultNameColumnName;

        /// <summary>
        /// Gets the Value Prefix from the specified <paramref name="globalConfig"/>, 
        /// <paramref name="typedConfig"/> and <paramref name="options"/> for the specified 
        /// <paramref name="typeName"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="typedConfig">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <param name="typeName">A <see cref="string"/> containing the type-name.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetValuePrefix(IEnumerable<IGlobalConfig> globalConfig, IEnumerable<ITypedConfig> typedConfig, Options options, string typeName) =>
            typedConfig.GetTyped(typeName, Configurations.ValuesPrefix)?.Value ??
            globalConfig.GetGlobal(Configurations.ValuesPrefix)?.Value ??
            options.ValuesPrefix;

        /// <summary>
        /// Get all the Value Prefixes from the specified <paramref name="globalConfig"/>, 
        /// <paramref name="typedConfig"/> and <paramref name="options"/>.
        /// </summary>
        /// <param name="globalConfig">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</param>
        /// <param name="typedConfig">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/> objects.</param>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string[] GetValuePrefixes(IEnumerable<IGlobalConfig> globalConfig, IEnumerable<ITypedConfig> typedConfig, Options options) =>
            GetAllItems(globalConfig, typedConfig, options.ValuesPrefix, Configurations.ValuesPrefix);

        private static string[] GetAllItems(IEnumerable<IGlobalConfig> globalConfig, IEnumerable<ITypedConfig> typedConfig, string optionsItemValue, string configurationKey)=>
            new List<string?>(
                typedConfig.GetTyped(configurationKey)
                    .Select(config => config.Value) )
            {
                globalConfig.GetGlobal(configurationKey)?.Value,
                optionsItemValue
            }.NotNull()
            .ToArray();
    }
}
