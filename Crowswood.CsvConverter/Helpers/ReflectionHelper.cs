using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Helpers
{
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Gets a <see cref="string"/> that represents the method arguments for the specified 
        /// <paramref name="argumentTypes"/>.
        /// </summary>
        /// <param name="argumentTypes">A <see cref="Type[]"/> containing the types of the arguments.</param>
        /// <returns>A <see cref="string"/>.</returns>
        internal static string GetMethodArguments(Type[] argumentTypes) =>
            string.Join(", ", argumentTypes.Select(t => t.GetName()));

        /// <summary>
        /// Gets a string that represents the method signature for the specified method <paramref name="name"/>, 
        /// <paramref name="genericType"/>, <paramref name="returnType"/> and <paramref name="argumentTypes"/>.
        /// </summary>
        /// <param name="name">A <see cref="string"/> containing the name of the method.</param>
        /// <param name="genericType">A <see cref="type"/> containing the generic type of the method.</param>
        /// <param name="returnType">A <see cref="Type"/> containing the return type of the method.</param>
        /// <param name="argumentTypes">A <see cref="Type[]"/> containing the types of the arguments.</param>
        /// <returns>A <see cref="string"/>.</returns>
        internal static string GetMethodSignature(string name, Type genericType, Type returnType, Type[] argumentTypes) =>
            $"{name}<{genericType.Name}>({GetMethodArguments(argumentTypes)} : {returnType.GetName()}.";

    }
}
