using System.ComponentModel;
using System.Reflection;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extension class for type extension methods.
    /// </summary>
    /// <remarks>
    /// This extension class only contains methods that extend <see cref="Type"/>. Any other 
    /// extension methods that more generally support reflection should live in the 
    /// <see cref="ReflectionExtensions"/> class.
    /// </remarks>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets the Attributes that the <paramref name="type"/> has defined.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Attribute"/>.</returns>
        public static IEnumerable<Attribute> GetAttributes(this Type type)
        {
            var attributes = TypeDescriptor.GetAttributes(type);
            foreach (Attribute attribute in attributes)
                yield return attribute;
        }

        /// <summary>
        /// Extension method to determine and return the depth of the <see cref="Type"/> in the
        /// hierarchy.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>An <see cref="int"/>.</returns>
        public static int GetDepth(this Type type) =>
            type.BaseType is null ? 0 : type.BaseType.GetDepth() + 1;

        /// <summary>
        /// Retrieves an instance non-public generic method with a generic type parameter of
        /// <typeparamref name="TParam"/>, has a name of <paramref name="name"/> and accepts the 
        /// specified <paramref name="arguments"/> to process objects of type <paramref name="genericType"/>.
        /// </summary>
        /// <typeparam name="TParam">The generic type parameter to apply to the method.</typeparam>
        /// <typeparam name="TReturn">The type of the return value of the method.</typeparam>
        /// <param name="name">A <see cref="string"/> that contains the name of the method.</param>
        /// <param name="genericType">A <see cref="Type"/> that indicates the type of object that the method will handle.</param>
        /// <param name="arguments">An <see cref="object"/> array that contains the arguments to supply to the method.</param>
        /// <returns>A <see cref="MethodInfo"/>.</returns>
        public static MethodInfo GetGenericMethod(this Type type, string name, Type genericType, Type returnType, object[] arguments) =>
            type.GetGenericMethod(name, genericType, returnType, arguments.Select(argument => argument.GetType()).ToArray());

        /// <summary>
        /// Retrieves an instance non-public generic method with a generic type parameter of
        /// <typeparamref name="TParam"/>, has a name of <paramref name="name"/> and accepts the 
        /// arguments of the specified <paramref name="argumentTypes"/> to process objects of type 
        /// <paramref name="genericType"/>.
        /// </summary>
        /// <typeparam name="TParam">The generic type parameter to apply to the method.</typeparam>
        /// <typeparam name="TReturn">The type of the return value of the method.</typeparam>
        /// <param name="name">A <see cref="string"/> that contains the name of the method.</param>
        /// <param name="genericType">A <see cref="Type"/> that indicates the type of object that the method will handle.</param>
        /// <param name="argumentTypes">A <see cref="Type"/> array that contains the types of arguments to supply to the method.</param>
        /// <returns>A <see cref="MethodInfo"/>.</returns>
        /// <exception cref="InvalidOperationException">If the a matcing method is not found.</exception>
        public static MethodInfo GetGenericMethod(this Type type, string name, Type genericType, Type returnType, Type[] argumentTypes) =>
            type.GetGenericMethod(name,
                                  BindingFlags.Instance |
                                  BindingFlags.NonPublic,
                                  genericType,
                                  argumentTypes,
                                  returnType) ??
            throw new InvalidOperationException(
                $"Failed to bind {ReflectionHelper.GetMethodSignature(name, genericType, returnType, argumentTypes)}.");

        /// <summary>
        /// Extension method to return a fully constructed generic method from the <paramref name="type"/>
        /// that has the specified <paramref name="name"/> and matches the specified <paramref name="bindingFlags"/>,
        /// for the specified <paramref name="genericType"/> that matches the specified 
        /// <paramref name="argumentTypes"/> and <paramref name="returnType"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> from which to return the method.</param>
        /// <param name="name">A <see cref="string"/> that contains the name of the method.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> to use.</param>
        /// <param name="genericType">The <see cref="Type"/> of the generic type.</param>
        /// <param name="argumentTypes">A <see cref="Type[]"/> containing the argument types.</param>
        /// <param name="returnType">The <see cref="Type"/> of the return type.</param>
        /// <returns>A <see cref="MethodInfo"/> or null if the method is not found or could not be constructed.</returns>
        public static MethodInfo? GetGenericMethod(this Type type,
                                                   string name,
                                                   BindingFlags bindingFlags,
                                                   Type genericType,
                                                   Type[] argumentTypes,
                                                   Type returnType) =>
            type.GetGenericMethod(name, bindingFlags, new[] { genericType }, argumentTypes, returnType);

        /// <summary>
        /// Extension method to return a fully constructed generic method from the <paramref name="type"/>
        /// that has the specified <paramref name="name"/> and matches the specified <paramref name="bindingFlags"/>,
        /// for the specified <paramref name="genericTypes"/> that matches the specified 
        /// <paramref name="argumentTypes"/> and <paramref name="returnType"/>
        /// </summary>
        /// <param name="type">The <see cref="Type"/> from which to return the method.</param>
        /// <param name="name">A <see cref="string"/> that contains the name of the method.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> to use.</param>
        /// <param name="genericTypes">A <see cref="Type[]"/> containing the generic types.</param>
        /// <param name="argumentTypes">A <see cref="Type[]"/> containing the argument types.</param>
        /// <param name="returnType">The <see cref="Type"/> of the return type.</param>
        /// <returns>A <see cref="MethodInfo"/> or null if the method is not found or could not be constructed.</returns>
        public static MethodInfo? GetGenericMethod(this Type type,
                                                   string name,
                                                   BindingFlags bindingFlags,
                                                   Type[] genericTypes,
                                                   Type[] argumentTypes,
                                                   Type returnType)
        {
            // We need to check the method parameters AFTER constructing the generic method to be 
            // able to check parameter types, otherwise any parameters based on the type-parameter
            // won't have been constructed themselves. Because constructing a generic method is
            // fairly heavy, we check the number of arguments match the number of types before
            // doing the construction so that we can filter out any methods with the wrong number
            // of parameters.
            var methods =
                type.GetMethods(bindingFlags)
                    .Where(method => method.Name == name)
                    .Select(method => method.ConstructGenericMethod(genericTypes))
                    .NotNull()
                    .Where(method => method.CheckArguments(argumentTypes, returnType))
                    .ToList();
            var method =
                methods.Count == 1 ? methods.First() : null;
            return method;
        }

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
        /// Gets the properties of <paramref name="type"/> that occur in the specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the property names.</param>
        /// <returns>A <see cref="PropertyInfo[]"/>.</returns>
        public static PropertyInfo[] GetPropertiesByName(this Type type, string[] propertyNames)
        {
            var properties = type.GetReadWriteProperties(propertyNames);

            var results =
                propertyNames
                    .Select(propertyName =>
                        properties
                            .FirstOrDefault(property => property.Name == propertyName))
                    .NotNull()
                    .ToArray();
            return results;
        }

        /// <summary>
        /// Retrieves the public instance properties of <paramref name="type"/> that are both read 
        /// and write.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="PropertyInfo[]"/>.</returns>
        public static PropertyInfo[] GetReadWriteProperties(this Type type) =>
            type.GetProperties(BindingFlags.Instance |
                               BindingFlags.Public)
                .Where(property => property.CanRead && property.CanWrite)
                .ToArray();

        /// <summary>
        /// Retrieves the public instance properties of <paramref name="type"/> that are both read 
        /// and write and whos name is included in <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> that contains the property names.</param>
        /// <returns>A <see cref="PropertyInfo[]"/>.</returns>
        public static PropertyInfo[] GetReadWriteProperties(this Type type, string[] propertyNames) =>
            type.GetReadWriteProperties()
                .Where(property => propertyNames?.Contains(property.Name) ?? false)
                .ToArray();

        /// <summary>
        /// Gets the public read/write properties that are supported for serialization for 
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="PropertyInfo"/> array.</returns>
        public static PropertyInfo[] GetReadWritePropertiesByDepth(this Type type) =>
            type.GetReadWriteProperties()
                .Where(property => property.PropertyType == typeof(string) ||
                                   property.PropertyType.IsValueType &&
                                   property.PropertyType != typeof(DateTime))
                .Select(property => new { Property = property, Depth = property.GetTypeDepth(), })
                .OrderBy(n => n.Depth)
                .Select(n => n.Property)
                .ToArray();

        /// <summary>
        /// Get the static string fields of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="FieldInfo[]"/>.</returns>
        public static FieldInfo[] GetStaticStringFields(this Type type) =>
            type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(string))
                .ToArray();

        /// <summary>
        /// Gets the type name from either the <paramref name="type"/> or its 
        /// <see cref="CsvConverterClassAttribute"/> if any.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public static string GetTypeName(this Type type) =>
            type.GetCustomAttribute<CsvConverterClassAttribute>()?.Name ??
            type.Name;

        /// <summary>
        /// Gets the types that exist in the <see cref="Assembly"/> that contains <paramref name="type"/>
        /// that exist in the specified <paramref name="typeNames"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> that identifies the <see cref="Assembly"/>.</param>
        /// <param name="typeNames">A <see cref="string[]"/> containing the names of the types to retrieve.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/>.</returns>
        public static Type[] GetTypes(this Type type, string[] typeNames) =>
            Assembly.GetAssembly(type)?.GetTypes()
                .Select(type => new
                {
                    Type = type,
                    Attribute = type.GetCustomAttribute<CsvConverterClassAttribute>(),
                })
                .Where(item => typeNames.Contains(item.Type.Name) ||
                               typeNames.Contains(item.Attribute?.Name))
                .Select(item => item.Type)
                .ToArray() ?? Array.Empty<Type>();

        /// <summary>
        /// Gets the types that exist in the <see cref="Assembly"/> that contains the <paramref name="type"/>
        /// that have the specified <paramref name="typeName"/> and which can be assigned to the 
        /// <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <param name="typeName">A <see cref="string"/> containing the name of the desired type.</param>
        /// <returns>A <see cref="Type[]"/>.</returns>
        public static Type[] GetType(this Type type, string typeName) =>
            Assembly.GetAssembly(type)?.GetTypes()
                .Where(t => t.Name == typeName)
                .Where(t => t.IsAssignableTo(type))
                .ToArray() ?? Array.Empty<Type>();
    }
}
