﻿namespace Crowswood.CsvConverter.Interfaces
{
    /// <summary>
    /// Interface that describes a full fluent serialization object.
    /// </summary>
    public interface ISerialization
    {
        /// <summary>
        /// Specifies the <paramref name="key"/> and <paramref name="value"/> of global 
        /// configuration to include in the serialization.
        /// </summary>
        /// <param name="key">A <see cref="string"/> containing the key.</param>
        /// <param name="value">A <see cref="string"/> containing the value.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization GlobalConfig(string key, string  value);

        /// <summary>
        /// Specifies the <paramref name="globalConfiguration"/> to include in the serialization.
        /// </summary>
        /// <param name="globalConfiguration">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the global configuration.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization GlobalConfig(Dictionary<string, string> globalConfiguration);

        /// <summary>
        /// Specifies the <paramref name="key"/> and <paramref name="value"/> of a type 
        /// configuration for the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">The type to associate with this type configuration.</typeparam>
        /// <param name="key">A <see cref="string"/> containing the key.</param>
        /// <param name="value">A <see cref="string"/> containing the value.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypeConfig<TObject>(string key, string value)
            where TObject : class;

        /// <summary>
        /// Specifies the <paramref name="key"/> and <paramref name="value"/> of a type 
        /// configuration for the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="objectType">A <see cref="Type"/> associated with this type configuration.</param>
        /// <param name="key">A <see cref="string"/> containing the key.</param>
        /// <param name="value">A <see cref="string"/> containing the value.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypeConfig(Type type, string key, string value);

        /// <summary>
        /// Specifies the <paramref name="key"/> and <paramref name="value"/> of a type 
        /// configuration for the specified <paramref name="objectTypeName"/>.
        /// </summary>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the type to associate with this type configuration.</param>
        /// <param name="key">A <see cref="string"/> containing the key.</param>
        /// <param name="value">A <see cref="string"/> containing the value.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypeConfg(string typeName, string key, string value);

        /// <summary>
        /// Specifies the <paramref name="typeConfiguration"/> to include in the serialization for 
        /// the <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">The type to associate with this type configuration.</typeparam>
        /// <param name="typeConfiguration">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the type configuration.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypeConfig<TObject>(Dictionary<string, string> typeConfiguration)
            where TObject : class;

        /// <summary>
        /// Specifies the <paramref name="typeConfiguration"/> to include in the serialization for 
        /// the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/> associated with this type configuration.</param>
        /// <param name="typeConfiguration">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the type configuration.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypeConfig(Type type, Dictionary<string, string> typeConfiguration);

        /// <summary>
        /// Specifies the <paramref name="typeConfiguration"/> to include in the serialization for 
        /// the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the type to associate with this type configuration.</param>
        /// <param name="typeConfiguration">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the type configuration.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypeConfig(string typeName, Dictionary<string, string> typeConfiguration);

        /// <summary>
        /// Specifies the number of blank lines to include in the serialization.
        /// </summary>
        /// <param name="number">An <see cref="int"/> that contains the number of blank lines to include; defaults to one.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization BlankLine(int number = 1);

        /// <summary>
        /// Specifies the <paramref name="typeConversion"/> to include in the serialization.
        /// </summary>
        /// <param name="typeConversion">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the type conversion.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypeConversion(Dictionary<string, string> typeConversion);

        /// <summary>
        /// Specifies the <paramref name="valueConversion"/> to include in the serialization.
        /// </summary>
        /// <param name="valueConversion">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> that contains the value conversion.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization ValueConversion(Dictionary<string, string> valueConversion);

        /// <summary>
        /// Specifies the <paramref name="comment"/> and the <paramref name="commentPrefix"/> to 
        /// include in the serialization.
        /// </summary>
        /// <param name="commentPrefix">A <see cref="string"/> that contains the comment prefix; it must correspond to one of the defined comment prefixes.</param>
        /// <param name="comment">A <see cref="string"/> that contains the text of the comment.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization Comment(string commentPrefix, params string[] comments);

        /// <summary>
        /// Specifies the <paramref name="metadata"/> for <typeparamref name="TObject"/> to 
        /// include in the serialization.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that the metadata describes.</typeparam>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <typeparamref name="TMetadata"/> that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        /// <remarks>
        /// For use with typed object data.
        /// </remarks>
        ISerialization TypedMetadata<TMetadata, TObject>(IEnumerable<TMetadata> metadata)
            where TObject : class
            where TMetadata : class;

        /// <summary>
        /// Specifies the <paramref name="metadata"/> for the specified <paramref name="dataType"/> to 
        /// include in the serialization.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="dataType">The <see cref="Type"/> of the data that the metadata is attached to.</param>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <typeparamref name="TMetadata"/> that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        /// <remarks>
        /// For use with typed object data, but the use of <seealso cref="TypedMetadata{TObject, TMetadata}(IEnumerable{TMetadata})"/>
        /// is prefered as it gives better type safety.
        /// </remarks>
        ISerialization TypedMetadata<TMetadata>(Type dataType, IEnumerable<TMetadata> metadata)
            where TMetadata : class;

        /// <summary>
        /// Specifies the <paramref name="metadata"/> for the specified <paramref name="dataTypeName"/> 
        /// to include in the serialization.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="dataTypeName">A <see cref="string"/> that contains the name of the data that the metadata is attached ot.</param>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <typeparamref name="TMetadata"/> that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        /// <remarks>
        /// For use with typeless object data.
        /// Can also be used with typed object data, but the use of <seealso cref="TypedMetadata{TObject, TMetadata}(IEnumerable{TMetadata})"/>
        /// is prefered as it gives better type safety.
        /// </remarks>
        ISerialization TypedMetadata<TMetadata>(string dataTypeName, IEnumerable<TMetadata> metadata)
            where TMetadata : class;

        /// <summary>
        /// Specifies the <paramref name="metadata"/> that has the specified <paramref name="metadataPrefix"/> 
        /// to include in the serialization.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that the metadata describes.</typeparam>
        /// <param name="metadataPrefix">A <see cref="string"/> that contains the type name of the metadata.</param>
        /// <param name="metadata">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> containing the property name that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        /// <remarks>
        /// For use with typed object data.
        /// </remarks>
        ISerialization TypelessMetadata<TObject>(string metadataPrefix, Dictionary<string, string> metadata)
            where TObject : class;

        /// <summary>
        /// Specifies the <paramref name="metadata"/> that has the specified <paramref name="metadataPrefix"/> 
        /// and which is to be attached to the specified <paramref name="dataType"/> of object data to 
        /// include in the serialization.
        /// </summary>
        /// <param name="dataType">The <see cref="Type"/> of object that the metadata describes.</param>
        /// <param name="metadataPrefix">A <see cref="string"/> that contains the type name of the metadata.</param>
        /// <param name="metadata">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> containing the property name that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        /// <remarks>
        /// For use with typed object data, but the use of <seealso cref="TypelessMetadata{TObject}(string, Dictionary{string, string})"/>
        /// is prefered as it gives better type safety.
        /// </remarks>
        ISerialization TypelessMetadata(Type dataType, string metadataPrefix, Dictionary<string, string> metadata);

        /// <summary>
        /// Specifies the <paramref name="metadata"/> that has the specified <paramref name="metadataPrefix"/> 
        /// for the specified <paramref name="dataTypeName"/> to include in the serialization.
        /// </summary>
        /// <param name="dataTypeName">A <see cref="string"/> that contains the type name of the object that the metadata describes.</param>
        /// <param name="metadataPrefix">A <see cref="string"/> that contains the prefix of the metadata.</param>
        /// <param name="metadata">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> containing the property name that contains the metadata.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        /// <remarks>
        /// For use with typeless object data.
        /// Can also be used with typed object data, but the use of <seealso cref="TypelessMetadata{TObject}(string, Dictionary{string, string})"/>
        /// is prefered as it gives better type safety.
        /// </remarks>
        ISerialization TypelessMetadata(string dataTypeName, string metadataPrefix, Dictionary<string, string> metadata);

        /// <summary>
        /// Specifies the <paramref name="data"/> to include in the serialization.
        /// </summary>
        /// <typeparam name="Tobject">The type of the data.</typeparam>
        /// <param name="data">An <see cref="IEnumerable{T}"/> of <typeparamref name="Tobject"/> that contains the data.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypedData<Tobject>(IEnumerable<Tobject> data)
            where Tobject : class;

        /// <summary>
        /// Specifies the <paramref name="data"/> for the specified <paramref name="typeName"/> to 
        /// include in the serialization.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> that contains the type name of the data.</param>
        /// <param name="names">A <see cref="string[]"/> that contains the property names of the data.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values of the data.</param>
        /// <returns>An <see cref="ISerialization"/> object to allow chaining.</returns>
        ISerialization TypelessData(string typeName, string[] names, IEnumerable<string[]> values);

        /// <summary>
        /// Serializes the previously specified data to a string.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the serialized data.</returns>
        string ToString();
    }
}
