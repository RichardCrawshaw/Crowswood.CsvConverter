using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Helpers
{
    /// <summary>
    /// An internal class that manages the conversions that use <see cref="ConversionType"/> and 
    /// <see cref="ConversionValue"/>.
    /// </summary>
    internal static class ConversionHelper
    {
        /// <summary>
        /// Gets the conversion types from the specified <paramref name="items"/> using the 
        /// specified <paramref name="configHandler"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that holds the text of the conversion types.</param>
        /// <param name="configHandler">A <see cref="ConfigHandler"/> object.</param>
        /// <returns>A <see cref="ConversionType"/> array.</returns>
        public static ConversionType[] GetConversionTypes(IEnumerable<string[]> items,
                                                          ConfigHandler configHandler) =>
            items
                .Where(items => items[0] == configHandler.GetConversionTypePrefix())
                .Select(items => new ConversionType 
                { 
                    OriginalTypeName = items[1].Trim().Trim('"'),
                    ConvertedTypeName = items[2].Trim().Trim('"'),
                })
                .ToArray();

        /// <summary>
        /// Gets the conversion values from the specified <paramref name="items"/> using the 
        /// specified <paramref name="configHandler"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that holds the text of the conversion values.</param>
        /// <param name="configHandler">A <see cref="ConfigHandler"/> object.</param>
        /// <returns>A <see cref="ConversionValue"/> array.</returns>
        public static ConversionValue[] GetConversionValues(IEnumerable<string[]> items,
                                                            ConfigHandler configHandler) =>
            items
                .Where(items => items[0] == configHandler.GetConversionValuePrefix())
                .Select(items => new ConversionValue
                {
                    OriginalValue = items[1].Trim().Trim('"'),
                    ConvertedValue = items[2].Trim().Trim('"'),
                })
                .ToArray();
    }
}
