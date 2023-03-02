using System.Reflection;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extension class to aid with reflection.
    /// </summary>
    public static class ReflectionExtensions
    {
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
                var property = ConversionHelper.GetProperty(members, propertyAndNamePairs, names[index]);
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
