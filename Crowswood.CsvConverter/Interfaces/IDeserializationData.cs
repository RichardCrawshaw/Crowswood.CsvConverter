namespace Crowswood.CsvConverter.Interfaces
{
    /// <summary>
    /// An interface to assist with retrieving the deserialized data and metadata.
    /// </summary>
    public interface IDeserializationData
    {
        /// <summary>
        /// Gets the deserialized typed <typeparamref name="TObject"/> data.
        /// </summary>
        /// <typeparam name="TObject">The type of data to retrieve.</typeparam>
        /// <returns>A <see cref="List{T}"/> of <typeparamref name="TObject"/>.</returns>
        List<TObject> GetData<TObject>()
            where TObject : class, new();

        /// <summary>
        /// Gets the deserialized typed object data that has the specified <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">A <see cref="Type"/> that indicates the type of data to return.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="object"/>.</returns>
        List<object> GetData(
            Type objectType);

        /// <summary>
        /// Gets the deserialized typed object data that has the specified <paramref name="objectType"/> 
        /// placing the all metadata into <paramref name="metadata"/>.
        /// </summary>
        /// <typeparam name="TMetadata">The type of metadata to retrieve.</typeparam>
        /// <param name="objectType">A <see cref="Type"/> that indicates the type of data to return.</param>
        /// <param name="metadata">A <see cref="List{T}"/> of <typeparamref name="TMetadata"/> that will receive the metadata.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="object"/>.</returns>
        List<object> GetData<TMetadata>(
            Type objectType, 
            out List<TMetadata> metadata)
            where TMetadata : class, new();

        /// <summary>
        /// Gets the deserialized typeless data for the specified <paramref name="objectTypeName"/>.
        /// </summary>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data-type to get.</param>
        /// <returns>A tuple of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.</returns>
        (string[] Names, IEnumerable<string[]> Values) GetData(
            string objectTypeName);

        /// <summary>
        /// Gets the deserialized typed <typeparamref name="TObject"/> data along with the 
        /// deserialized typed <typeparamref name="TMetadata"/> metadata.
        /// </summary>
        /// <typeparam name="TObject">The type of data to retrieve.</typeparam>
        /// <typeparam name="TMetadata">The type of metadata to retrieve.</typeparam>
        /// <param name="metadata">A <see cref="List{T}"/> of <typeparamref name="TMetadata"/> to receive the metadata.</param>
        /// <returns></returns>
        List<TObject> GetData<TObject, TMetadata>(
            out List<TMetadata> metadata)
            where TObject : class, new()
            where TMetadata : class, new();

        /// <summary>
        /// Gets the deserialized types object data of the specified <paramref name="objectType"/> 
        /// along with the deserialized metadata identified by the specified <paramref name="metadataTypeName"/> 
        /// into the <paramref name="metadata"/>.
        /// </summary>
        /// <param name="objectType">A <see cref="Type"/> that indicates the type of data to return.</param>
        /// <param name="metadataTypeName">A <see cref="string"/> containing the name of the metadata type to get.</param>
        /// <param name="metadata">A <see cref="List{T}"/> of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values of the metadata.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.</returns>
        List<object> GetData(
            Type objectType, string metadataTypeName,
            out (string[] Names, IEnumerable<string[]> Values) metadata);

        /// <summary>
        /// Gets the deserialized typeless data for the specified <paramref name="objectTypeName"/> 
        /// and the corresponding typeless metadata for the specified <paramref name="metadataTypeName"/> 
        /// into <paramref name="metadata"/>.
        /// </summary>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data-type to get.</param>
        /// <param name="metadataTypeName">A <see cref="string"/> containing the name of the metadata type to get.</param>
        /// <param name="metadata">A <see cref="List{T}"/> of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values of the metadata.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.</returns>
        (string[] Names, IEnumerable<string[]> Values) GetData(
            string objectTypeName, string metadataTypeName, 
            out (string[] Names, IEnumerable<string[]> Values) metadata);

        /// <summary>
        /// Gets the deserialized typed <typeparamref name="TObject"/> data along with all the 
        /// metadata into <paramref name="metadata"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of data to retrieve.</typeparam>
        /// <param name="metadata">A <see cref="List{T}"/> of object that will receive the metadata.</param>
        /// <returns>A <see cref="List{T}"/> of <typeparamref name="TObject"/>.</returns>
        List<TObject> GetData<TObject>(
            out List<object> metadata)
            where TObject : class, new();

        /// <summary>
        /// Gets the deserialized typeless data for the specified <paramref name="objectTypeName"/>
        /// along with the the <typeparamref name="TMetadata"/> metadata into <paramref name="metadata"/>.
        /// </summary>
        /// <typeparam name="TMetadata">The type of metadata to retrieve.</typeparam>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data-type to get.</param>
        /// <param name="metadata">A <see cref="List{T}"/> of object that will receive the metadata.</param>
        /// <returns>A tuple containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.</returns>
        (string[] Names, IEnumerable<string[]> Values) GetData<TMetadata>(
            string objectTypeName, 
            out List<TMetadata> metadata)
            where TMetadata : class, new();

        List<TObject> GetData<TObject>(
            string metadataTypeName, 
            out (string[] Names, IEnumerable<string[]> Values) metadata)
            where TObject : class, new();

        /// <summary>
        /// Retrieves a list of the object types that are available.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="Type"/>.</returns>
        List<Type> GetObjectTypes();

        /// <summary>
        /// Retrieves a list of the object type names that are available.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        List<string> GetObjectTypeNames();

    }
}
