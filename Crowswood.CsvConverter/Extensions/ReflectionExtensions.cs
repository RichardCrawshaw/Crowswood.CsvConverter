using System.Reflection;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extension class to aid with reflection.
    /// </summary>
    /// <remarks>
    /// This extension class only holds general reflection methods; it does not contain any 
    /// methods that extend <see cref="Type"/>. Those should live in the <see cref="TypeExtensions"/> 
    /// class.
    /// </remarks>
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Retrieves and returns the values of the properties of the specified <paramref name="item"/> 
        /// that are in the <paramref name="properties"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of item to process.</typeparam>
        /// <param name="item">A <typeparamref name="TBase"/> object.</param>
        /// <param name="properties">A <see cref="PropertyInfo"/> array.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/> and <see cref="object"/>.</returns>
        internal static IEnumerable<(Type Type, object? Value)> AsStrings<TBase>(this TBase item, PropertyInfo[] properties)
            where TBase : class =>
            properties
                .Select(property => (Type: property.PropertyType, Value: property.GetValue(item)));

        /// <summary>
        /// Checks and returns whether the <paramref name="method"/> has arguments that match the
        /// number and order of the specified <paramref name="types"/> and whether the return type
        /// matches the specified <paramref name="returnType"/>.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo"/> object.</param>
        /// <param name="types">A <see cref="Type"/> array.</param>
        /// <param name="returnType">A <see cref="Type"/>.</param>
        /// <returns>True if the types match the arguments and the return type matches; otherwise false.</returns>
        public static bool CheckArguments(this MethodInfo method, Type[] types, Type returnType) =>
            method.GetParameters().CheckArgumentTypes(types) &&
            method.ReturnType.IsAssignableFrom(returnType);

        /// <summary>
        /// Checks whether the <paramref name="parameters"/> have types that match the specified
        /// <paramref name="types"/>.
        /// </summary>
        /// <param name="parameters">The <see cref="ParameterInfo"/> array.</param>
        /// <param name="types">A <see cref="Type"/> array.</param>
        /// <returns>True if the number and order of the parameters and types match; false otherwise.</returns>
        public static bool CheckArgumentTypes(this ParameterInfo[] parameters, Type[] types) =>
            parameters.Length == types.Length &&
            Enumerable
                .Range(0, parameters.Length)
                .All(index => parameters[index].ParameterType.IsAssignableFrom(types[index]));

        /// <summary>
        /// Construct a generic method from the <paramref name="method"/> and the specified 
        /// <paramref name="genericTypes"/>.
        /// </summary>
        /// <param name="method">A <see cref="MethodInfo"/> object.</param>
        /// <param name="genericTypes">A <see cref="Type"/> array containing the types of the required generic method.</param>
        /// <returns>A generic method, or null if one could not be constructed.</returns>
        public static MethodInfo? ConstructGenericMethod(this MethodInfo method, params Type[] genericTypes)
        {
            try
            {
                if (!method.IsGenericMethod)
                    return null;
                if (method.GetGenericArguments().Length != genericTypes.Length)
                    return null;
                return method.MakeGenericMethod(genericTypes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the types of the <paramref name="attributes"/>.
        /// </summary>
        /// <param name="attributes">An <see cref="IEnumerable{T}"/> of <see cref="Attribute"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/>.</returns>
        public static IEnumerable<Type> GetAttributeTypes(this IEnumerable<Attribute> attributes) =>
            attributes
                .Select(attribute => attribute.GetType())
                .Distinct();

        /// <summary>
        /// Retrieves the <see cref="PropertyAndAttributePair"/> objects for the <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">A <see cref="PropertyInfo[]"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="PropertyAndAttributePair"/> objects.</returns>
        public static IEnumerable<PropertyAndAttributePair> GetPropertyAndAttributePairs(this PropertyInfo[] properties) =>
            properties
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

        /// <summary>
        /// Extension method to determine and return the depth of the type that declares the 
        /// <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/>.</param>
        /// <returns>An <see cref="int"/>.</returns>
        public static int GetTypeDepth(this PropertyInfo property) =>
            property.DeclaringType?.GetDepth() ?? 0;

        /// <summary>
        /// Gets the values of the properties from the specified <paramref name="item"/> for the  
        /// specified <paramref name="propertyNames"/> as a <see cref="string[]"/> including 
        /// adding leading and trailing double-quote marks.
        /// </summary>
        /// <param name="item">An <see cref="object"/>.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the name of the properties.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        public static string[] GetValues(this object item, string[] propertyNames) =>
            item.GetValues(propertyNames, item.GetType().GetProperties());

        /// <summary>
        /// Gets the values of the properties from the specified <paramref name="item"/> using 
        /// the specified <paramref name="properties"/> that are named in the specified <paramref name="propertyNames"/>.
        /// as a <see cref="string[]"/> including adding leading and trailing double-quote marks.
        /// </summary>
        /// <param name="item">An <see cref="object"/>.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the names of the properties that are to be included.</param>
        /// <param name="properties">A <see cref="PropertyInfo[]"/> containing the properties to retrieve.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        public static string[] GetValues(this object item, string[] propertyNames, PropertyInfo[] properties) =>
            propertyNames
                .Select(propertyName => properties.FirstOrDefault(property => property.Name == propertyName))
                .Select(property => property?.GetValue(item)?.ToString() ?? string.Empty)
                .Select(value => $"\"{value}\"")
                .ToArray();

        /// <summary>
        /// Determine and return whether <paramref name="attribute"/> may be applied to a class.
        /// </summary>
        /// <param name="attribute">An <see cref="Attribute"/>.</param>
        /// <returns>True if it is; false otherwise.</returns>
        public static bool IsClassAttribute(this Attribute attribute) =>
            attribute
                .GetType()
                .GetCustomAttributes<AttributeUsageAttribute>(true)
                .Any(a => (a.ValidOn & AttributeTargets.Class) == AttributeTargets.Class);

        /// <summary>
        /// Sets the properties on <paramref name="result"/> for the specified <paramref name="members"/>
        /// and <paramref name="propertyAndNamePairs"/> indicated by the specified <paramref name="names"/>
        /// using the specified <paramref name="valueConverter"/> to convert the <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to handle.</typeparam>
        /// <param name="result">The <typeparamref name="T"/> to update.</param>
        /// <param name="valueConverter">A <see cref="ValueConverter"/> to convert the values.</param>
        /// <param name="members">A <see cref="OptionMember"/> array.</param>
        /// <param name="propertyAndNamePairs">An <see cref="IEnumerable{T}"/> of <see cref="PropertyAndNamePair"/> objects.</param>
        /// <param name="names">A <see cref="string{}"/> containing the names to updae.</param>
        /// <param name="values">A <see cref="string[]"/> containing the values.</param>
        /// <returns>The <paramref name="result"/>.</returns>
        /// <exception cref="ArgumentException">If the <paramref name="names"/> length and <paramref name="values"/> length are different.</exception>
        public static T SetProperties<T>(this T result,
                                         ValueConverter valueConverter,
                                         OptionMember[] members,
                                         IEnumerable<PropertyAndNamePair> propertyAndNamePairs,
                                         string[] names,
                                         string[] values)
            where T : class
        {
            if (names.Length != values.Length)
                throw new ArgumentException(
                    $"The length of {nameof(names)} ({string.Join("|", names)}) and {nameof(values)} ({string.Join("|", values)}) do not match.");

            var properties = new List<PropertyInfo?>();
            for (var index = 0; index < values.Length; index++)
            {
                // Get the PropertyInfo to use to set the value for the name, based on the
                // OptionMembers and PropertyAnNamePairs.
                var property =
                    members
                        .Where(member => member.Name == names[index])
                        .Select(member => member.Property)
                        .FirstOrDefault() ??
                    propertyAndNamePairs
                        .Where(item => item.Name == names[index])
                        .Select(item => item.Property)
                        .FirstOrDefault() ??
                    propertyAndNamePairs
                        .Where(item => item.Property.Name == names[index])
                        .Select(item => item.Property)
                        .FirstOrDefault();
                //ConverterHelper.GetProperty(members, propertyAndNamePairs, names[index]);
                properties.Add(property);
            }

            result.SetProperties(valueConverter, properties.ToArray(), names, values);

            return result;
        }

        /// <summary>
        /// Sets the properties on <paramref name="result"/> for the specified <paramref name="properties"/> 
        /// identified by the specified <paramref name="names"/> using the <paramref name="valueConverter"/> 
        /// to convert the <paramref name="values"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="valueConverter"></param>
        /// <param name="properties"></param>
        /// <param name="names"></param>
        /// <param name="values"></param>
        /// <returns>The <paramref name="result"/>.</returns>
        public static T SetProperties<T>(this T result, ValueConverter valueConverter, PropertyInfo?[] properties, string[] names, string[] values)
            where T : class
        {
            for (var index = 0; index < names.Length && index < values.Length && index < properties.Length; index++)
            {
                var property = properties[index];
                if (property is null) continue;
                property.SetValue(result, valueConverter.ConvertValue(values[index], property.PropertyType));
            }

            return result;
        }
    }
}
