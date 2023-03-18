namespace Crowswood.CsvConverter.Interfaces
{
    public interface ISerialization
    {
        /// <summary>
        /// Gets the converter that will be handling the conversion.
        /// </summary>
        Converter Converter { get; }

        /// <summary>
        /// Specifies the <paramref name="globalConfiguration"/> to include in the serialization.
        /// </summary>
        /// <param name="globalConfiguration">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the global configuration.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization GlobalConfig(Dictionary<string, string> globalConfiguration);

        /// <summary>
        /// Specifies the <paramref name="typeConfiguration"/> to include in the serialization.
        /// </summary>
        /// <typeparam name="TObject">The type to associate with this type configuration.</typeparam>
        /// <param name="typeConfiguration">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the type configuration.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization TypeConfig<TObject>(Dictionary<string, string> typeConfiguration);

        /// <summary>
        /// Specifies the number of blank lines to include in the serialization.
        /// </summary>
        /// <param name="number">An <see cref="int"/> that contains the number of blank lines to include; defaults to one.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization BlankLine(int number = 1);

        /// <summary>
        /// Specifies the <paramref name="typeConversion"/> to include in the serialization.
        /// </summary>
        /// <param name="typeConversion">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the type conversion.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization TypeConversion(Dictionary<string, string> typeConversion);

        /// <summary>
        /// Specifies the <paramref name="valueConversion"/> to include in the serialization.
        /// </summary>
        /// <param name="valueConversion">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the value conversion.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization ValueConversion(Dictionary<string, string> valueConversion);

        /// <summary>
        /// Specifies the <paramref name="comment"/> and the <paramref name="commentPrefix"/> to 
        /// include in the serialization.
        /// </summary>
        /// <param name="commentPrefix">A <see cref="string"/> that contains the comment prefix; it must correspond to one of the defined comment prefixes.</param>
        /// <param name="comment">A <see cref="string"/> that contains the text of the comment.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization Comment(string commentPrefix, string comment);

        /// <summary>
        /// Specifies the <paramref name="metadata"/> for <typeparamref name="TObject"/> to 
        /// include in the serialization.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that the metadata describes.</typeparam>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <typeparamref name="TMetadata"/> that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization TypedMetadata<TObject, TMetadata>(IEnumerable<TMetadata> metadata)
            where TObject : class
            where TMetadata : class;

        /// <summary>
        /// Specifies the <paramref name="metadata"/> that has the specified <paramref name="metadataTypeName"/> 
        /// to include in the serialization.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that the metadata describes.</typeparam>
        /// <param name="metadataTypeName">A <see cref="string"/> that contains the type name of the metadata.</param>
        /// <param name="metadata">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> containing the property name that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization TypelessMetadata<TObject>(string metadataTypeName, Dictionary<string, string> metadata) =>
            TypelessMetadata(typeof(TObject).Name, metadataTypeName, metadata);

        /// <summary>
        /// Specifies the <paramref name="metadata"/> that has the specified <paramref name="metadataPrefix"/> 
        /// for the specified <paramref name="typeName"/> to include in the serialization.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the type name of the object that the metadata describes.</param>
        /// <param name="metadataPrefix">A <see cref="string"/> that contains the prefix of the metadata.</param>
        /// <param name="metadata">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> containing the property name that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization TypelessMetadata(string typeName, string metadataPrefix, Dictionary<string,string> metadata);

        /// <summary>
        /// Specifies the <paramref name="data"/> to include in the serialization.
        /// </summary>
        /// <typeparam name="Tobject">The type of the data.</typeparam>
        /// <param name="data">An <see cref="IEnumerable{T}"/> of <typeparamref name="Tobject"/> that contains the data.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization TypedData<Tobject>(IEnumerable<Tobject> data)
            where Tobject : class;

        /// <summary>
        /// Specifies the <paramref name="data"/> for the specified <paramref name="typeName"/> to 
        /// include in the serialization.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the type name of the data.</param>
        /// <param name="names">A <see cref="string[]"/> that contains the property names of the data.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values of the data.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        public ISerialization TypelessData(string typeName, string[] names, IEnumerable<string[]> values);

        /// <summary>
        /// Serializes the previously specified data.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the serialized data.</returns>
        public string Serialize();
    }
}
