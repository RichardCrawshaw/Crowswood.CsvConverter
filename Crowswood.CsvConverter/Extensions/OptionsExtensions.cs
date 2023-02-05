using System.Reflection;

namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extensions class for the <see cref="Options"/> object hierarchy.
    /// </summary>
    public static class OptionsExtensions
    {
        /// <summary>
        /// Retrieves the properties of the <seealso cref="OptionMetadata.Type"/> that match the
        /// <seealso cref="OptionMetadata.PropertyNames"/>.
        /// </summary>
        /// <param name="optionMetadata">The <see cref="OptionMetadata"/> object.</param>
        /// <returns>A <see cref="PropertyInfo"/> array.</returns>
        public static PropertyInfo[] GetProperties(this OptionMetadata optionMetadata) =>
            optionMetadata.Type.GetProperties()
                .Where(property => optionMetadata.PropertyNames.Contains(property.Name))
                .ToArray();
    }
}
