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
        /// Gets the <see cref="OptionMember"/> element from the <paramref name="options"/> that 
        /// has the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <param name="name">A <see cref="string"/> that contains the name.</param>
        /// <returns>A <see cref="OptionMember"/> object, or null.</returns>
        public static OptionMember? GetOptionMember(this Options options, string name) =>
            options.OptionMembers
                .FirstOrDefault(om => om.Name == name);

        /// <summary>
        /// Gets the <see cref="OptionMember"/> element from the <paramref name="options"/> that 
        /// has a property with the same name as the specified <paramref name="property"/>.
        /// </summary>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <param name="property">A <see cref="PropertyInfo"/> object.</param>
        /// <returns>A <see cref="OptionMember"/> object, or null.</returns>
        public static OptionMember? GetOptionMember(this Options options, PropertyInfo property) =>
            options.OptionMembers
                .FirstOrDefault(om => om.Property.Name == property.Name);

        /// <summary>
        /// Gets the <see cref="OptionMember"/> element from the <paramref name="options"/> that 
        /// has the specified <paramref name="type"/>,
        /// </summary>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="OptionMember"/> object, or null.</returns>
        public static OptionMember? GetOptionMember(this Options options, Type type) =>
            options.OptionMembers
                .FirstOrDefault(om => om.Type == type);

        /// <summary>
        /// Gets the <see cref="OptionMetadata"/> that has the <seealso cref="OptionMetadata.Prefix"/> 
        /// that matches the specified <paramref name="prefix"/>.
        /// </summary>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <param name="prefix">A <see cref="string"/> that contains the prefix used to identify the metadata.</param>
        /// <returns>An <see cref="OptionMetadata"/> object; or null if none match.</returns>
        public static OptionMetadata? GetOptionMetadata(this Options options, string prefix) =>
            options.OptionMetadata
                .FirstOrDefault(metadata => metadata.Prefix == prefix);

        /// <summary>
        /// Gets the <see cref="OptionMetadata"/> that has the <seealso cref="OptionMetadata.Type"/> 
        /// that matches the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>An <see cref="OptionMetadata"/> object; or null if none match.</returns>
        public static OptionMetadata? GetOptionMetadata(this Options options, Type type) =>
            options.OptionMetadata
                .FirstOrDefault(metadata => metadata.Type == type);

        /// <summary>
        /// Gets the property names from the <paramref name="properties"/> with any conversions 
        /// that are defined in the <seealso cref="options"/> or as an attribute on the member 
        /// itself.
        /// </summary>
        /// <param name="properties">A <see cref="PropertyInfo[]"/> that contains the properties.</param>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <returns>A <see cref="string[]"/> containing the names.</returns>
        public static string[] GetParameters(this PropertyInfo[] properties, Options options) =>
            properties
                .Select(property =>
                    options.GetOptionMember(property)?.Name ??
                    property.GetCustomAttribute<CsvConverterPropertyAttribute>()?.Name ??
                    property.Name)
                .ToArray();

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

        /// <summary>
        /// Gets the property names from <paramref name="optionMetadata"/> for the specified 
        /// <paramref name="typeName"/>.
        /// </summary>
        /// <param name="optionMetadata">The <see cref="OptionMetadata"/> array.</param>
        /// <param name="typeName">A <see cref="string"/> that contains the name of the type.</param>
        /// <returns>A <see cref="string[]"/> that can be null.</returns>
        public static string[]? GetPropertyNames(this OptionMetadata[] optionMetadata, string typeName) =>
            optionMetadata
                .Where(om => om.Type.Name == typeName)
                .Select(om => om.PropertyNames)
                .FirstOrDefault();

        /// <summary>
        /// Gets the <see cref="OptionType"/> element from the <paramref name="options"/> that has 
        /// the specified <paramref name="name"/>.
        /// </summary>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <param name="name">A <see cref="string"/> that contains the name.</param>
        /// <returns>A <see cref="OptionType"/> object, or null.</returns>
        public static OptionType? GetOptionType(this Options options, string name) =>
            options.OptionTypes
                .FirstOrDefault(ot=>ot.Name == name);

        /// <summary>
        /// Gets the <see cref="OptionType"/> element from the <paramref name="options"/> that has 
        /// the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="options">The <see cref="Options"/> object.</param>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="OptionType"/> object, or null.</returns>
        public static OptionType? GetOptionType(this Options options, Type type) =>
            options.OptionTypes
                .FirstOrDefault(ot => ot.Type == type);
    }
}
