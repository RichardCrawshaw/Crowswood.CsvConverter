using System.Reflection;

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
            var methods =
                type.GetMethods(bindingFlags)
                    .Where(method => method.Name == name)
                    .Where(method => method.IsGenericMethod)
                    .Where(method => method.GetGenericArguments().Length == genericTypes.Length)
                    .Select(method => method.MakeGenericMethod(genericTypes))
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
    }
}
