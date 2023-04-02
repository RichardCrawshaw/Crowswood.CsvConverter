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
        /// Converts the specified <paramref name="typeName"/> considering the specified 
        /// <paramref name="isEnabled"/> flag and using the specified <paramref name="conversionTypes"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the typename to convert.</param>
        /// <param name="isEnabled">True to perform the conversion; false otherwise.</param>
        /// <param name="conversionTypes">A <see cref="ConversionType[]"/> containing the conversion types.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string ConvertType(string typeName, bool isEnabled, ConversionType[] conversionTypes) =>
            isEnabled ?
            conversionTypes
                .Where(ct => ct.OriginalTypeName == typeName)
                .Select(ct => ct.ConvertedTypeName)
                .FirstOrDefault() ?? typeName : typeName;

        /// <summary>
        /// Converts the specified <paramref name="value"/> considering the specified <paramref name="isEnabled"/>
        /// flag and using the specified <paramref name="conversionValues"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> containing the value to convert.</param>
        /// <param name="isEnabled">True to perform the conversion; false otherwise.</param>
        /// <param name="conversionValues">A <see cref="ConversionValue[]"/> containing the conversion values.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string ConvertValue(string value, bool isEnabled, ConversionValue[] conversionValues) =>
            isEnabled ?
            conversionValues
                .Where(cv => cv.OriginalValue == value)
                .Select(cv => cv.ConvertedValue)
                .FirstOrDefault() ?? value : value;

        /// <summary>
        /// Gets the conversion types from the specified <paramref name="items"/> using the 
        /// specified <paramref name="configHandler"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> that holds the text of the conversion types.</param>
        /// <param name="configHandler">A <see cref="ConfigHandler"/> object.</param>
        /// <returns>A <see cref="ConversionType"/> array.</returns>
        public static ConversionType[] GetConversionTypes(IEnumerable<string[]> items,
                                                          ConfigHandler configHandler) =>
            GetConversionTypes(items, configHandler.GetConversionTypePrefix());

        public static ConversionType[] GetConversionTypes(IEnumerable<string[]> items, string prefix) =>
            items
                .Where(items => items[0]== prefix)
                .Select(items=> new ConversionType
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
            GetConversionValues(items, configHandler.GetConversionValuePrefix());
        public static ConversionValue[] GetConversionValues(IEnumerable<string[]> items, string prefix)=>
            items
                .Where(items => items[0]==prefix)
                .Select(items => new ConversionValue
                {
                    OriginalValue = items[1].Trim().Trim('"'),
                    ConvertedValue = items[2].Trim().Trim('"'),
                })
                .ToArray();
    }
}
