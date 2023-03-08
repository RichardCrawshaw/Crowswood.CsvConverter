using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.UserConfig;

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

    }
}
