using System.Reflection;

namespace Crowswood.CsvConverter.Extensions
{
    /// <summary>
    /// An extension class for <see cref="MethodInfo"/> extension methods to aid with reflection.
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
    }
}
