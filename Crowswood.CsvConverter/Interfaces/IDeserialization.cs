using System.Reflection;

namespace Crowswood.CsvConverter.Interfaces
{
    /// <summary>
    /// Interface that supports the registratin of object data and metadata for deserialization.
    /// </summary>
    public interface IDeserialization
    {
        /// <summary>
        /// Register the <typeparamref name="TObject"/> object data for deserialization.
        /// </summary>
        /// <typeparam name="TObject">The type of object to be deserialized.</typeparam>
        /// <returns>An <see cref="IDeserialization"/> object to allow chaining.</returns>
        IDeserialization Data<TObject>() where TObject : class, new();

        /// <summary>
        /// Registers the specified <paramref name="objectTypes"/> to be deserialized.
        /// </summary>
        /// <param name="objectTypes">A <see cref="Type[]"/> containing the object types.</param>
        /// <returns>An <see cref="IDeserialization"/> object to allow chaining.</returns>
        IDeserialization Data(params Type[] objectTypes);

        /// <summary>
        /// Registers the specified <paramref name="objectTypeNames"/> to be deserialized.
        /// </summary>
        /// <param name="objectTypeNames">A <see cref="string[]"/> containing the object type names.</param>
        /// <returns>An <see cref="IDeserialization"/> object to allow chaining.</returns>
        IDeserialization Data(params string[] objectTypeNames);

        /// <summary>
        /// Retrieves a list of the object types that are contained in the text.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="Type"/>.</returns>
        public List<Type> GetTypes() => GetTypes(Assembly.GetCallingAssembly());

        /// <summary>
        /// Retrieves a list of the object types that are contained in the text being deserialized 
        /// and which are defined in the specified <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="assemblies">An array of <see cref="Assembly"/> that define the expected types.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Type"/>.</returns>
        /// <remarks>
        /// If the <paramref name="assemblies"/> contain multiple types with the same name then 
        /// each will be included in the results if that name is included in the text being 
        /// deserialized.
        /// </remarks>
        public List<Type> GetTypes(params Assembly[] assemblies) =>
            GetTypes(
                assemblies
                    .Select(assembly => assembly.GetTypes())
                    .SelectMany(types => types)
                    .ToArray());

        /// <summary>
        /// Retrieves a list of the object types that are contained in the text being deserialized 
        /// and which are defined in the specified <paramref name="types"/>.
        /// </summary>
        /// <param name="types">An array of <see cref="Type"/> that define the expected types.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Type"/>.</returns>
        /// <remarks>
        /// If there are multiple types with the same name then each will be included in the 
        /// results if that name is included in the text being deserialized.
        /// </remarks>
        List<Type> GetTypes(params Type[] types);

        /// <summary>
        /// Retrieves a list of the object type names that are contained in the text.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        List<string> GetTypeNames();

        /// <summary>
        /// Ends the deserialization process.
        /// </summary>
        /// <returns>An <see cref="IDeserializationData"/> object to allow the retrieval of the deserialized data.</returns>
        IDeserializationData Deserialize();
    }
}
