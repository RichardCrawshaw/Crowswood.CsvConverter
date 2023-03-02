using System.Reflection;

namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extensions class for the <see cref="Options"/> object hierarchy.
    /// </summary>
    public static class OptionsExtensions
    {
        /// <summary>
        /// Gets the default reference element from the <paramref name="optionReferences"/>.
        /// </summary>
        /// <param name="optionReferences">An <see cref="OptionReference[]"/> containing the opiton refereces.</param>
        /// <returns>An <see cref="OptionReference"/>; null if there is no default defined.</returns>
        public static OptionReference? Get(this OptionReference[] optionReferences)=>
            optionReferences
                .FirstOrDefault(n => n.GetType() == typeof(OptionReference));

        /// <summary>
        /// Gets the reference element from the <paramref name="optionReferences"/> that matches 
        /// the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="optionReferences">An <see cref="OptionReference[]"/> containing the opiton refereces.</param>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type.</param>
        /// <returns>An <see cref="OptionReferenceType"/>; null if there is nothing defined for the <paramref name="typeName"/>.</returns>
        public static OptionReferenceType? Get(this OptionReference[] optionReferences, string typeName) =>
            optionReferences
                .Select(n => n as OptionReferenceType)
                .NotNull()
                .FirstOrDefault(n => n.TypeName == typeName);

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
