using System.Reflection;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extension class for type extension methods.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Extension method to determine and return the depth of the <see cref="Type"/> in the
        /// hierarchy.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>An <see cref="int"/>.</returns>
        public static int GetDepth(this Type type) =>
            type.BaseType is null ? 0 : type.BaseType.GetDepth() + 1;

        /// <summary>
        /// Extension method to return the <see cref="Type"/> name in the standard readable form.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>A <see cref="string"/>.</returns>
        /// <remarks>
        /// Uses recursion where there are generic types.
        /// </remarks>
        public static string GetName(this Type type)
        {
            if (type.IsGenericType)
            {
                var names =
                    type.GetGenericArguments()
                        .Select(t => t.GetName());
                var pos = type.Name.IndexOf('`');
                var baseName =
                    pos >= 0 ? type.Name[..pos] : type.Name;
                var name = $"{baseName}<{string.Join(",", names)}>";
                return name;
            }
            return type.Name;
        }

        /// <summary>
        /// Retrieves the <see cref="PropertyAndAttributePair"/> objects for the <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PropertyAndAttributePair"/> objects.</returns>
        public static IEnumerable<PropertyAndAttributePair> GetPropertyAndAttributePairs(this Type type) =>
            type.GetProperties(BindingFlags.Instance |
                               BindingFlags.Public)
                .Select(property => new PropertyAndAttributePair(property));

        /// <summary>
        /// Convert the <paramref name="propertyAndAttributePairs"/> into an <see cref="IEnumerable{T}"/>
        /// of <see cref="PropertyAndNamePair"/> objects.
        /// </summary>
        /// <param name="propertyAndAttributePairs">An <see cref="IEnumerable{T}"/> of <see cref="PropertyAndAttributePair"/> objects.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PropertyAndNamePair"/> objects.</returns>
        public static IEnumerable<PropertyAndNamePair> GetPropertyAndNamePairs(this IEnumerable<PropertyAndAttributePair> propertyAndAttributePairs) =>
            propertyAndAttributePairs
                .Select(item => new PropertyAndNamePair(item));
    }
}
