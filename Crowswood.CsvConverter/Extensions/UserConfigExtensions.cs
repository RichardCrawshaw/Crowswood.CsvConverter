using Crowswood.CsvConverter.Interfaces;

namespace Crowswood.CsvConverter.Extensions
{
    public static class UserConfigExtensions
    {
        /// <summary>
        /// Gets the <see cref="IGlobalConfig"/> item that has the specified <paramref name="name"/>, 
        /// or null if there is no match.
        /// </summary>
        /// <param name="config">An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/>.</param>
        /// <param name="name">A <see cref="string"/> that contains the name of a configuration item.</param>
        /// <returns>An <see cref="IGlobalConfig"/> or null.</returns>
        public static IGlobalConfig? GetGlobal(this IEnumerable<IGlobalConfig> config, string name) =>
            config.FirstOrDefault(cf => cf.Name == name);

        /// <summary>
        /// Gets the <see cref="ITypedConfig"/> item that has the specified <paramref name="typeName"/> 
        /// and <paramref name="name"/>, or null if there is no match.
        /// </summary>
        /// <param name="config">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/>.</param>
        /// <param name="typeName">A <see cref="string"/> that contains the name of a data-type.</param>
        /// <param name="name">A <see cref="string"/> that contains the name of a configuration item.</param>
        /// <returns>An <see cref="ITypedConfig"/> or null.</returns>
        public static ITypedConfig? GetTyped(this IEnumerable<ITypedConfig> config, string typeName, string name) =>
            config.FirstOrDefault(cf => cf.TypeName == typeName && cf.Name == name);

        /// <summary>
        /// Gets all the <see cref="ITypedConfig"/> items that have the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="config">An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/>.</param>
        /// <param name="name">A <see cref="string"/> that contains the name of a configuration item.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/>.</returns>
        public static IEnumerable<ITypedConfig> GetTyped(this IEnumerable<ITypedConfig> config, string name) =>
            config.Where(cf => cf.Name == name);
    }
}
