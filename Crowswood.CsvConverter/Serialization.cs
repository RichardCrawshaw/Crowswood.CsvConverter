using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Interfaces;
using Crowswood.CsvConverter.Serializations;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An internal class to manage the serialization of data using a fluent interface.
    /// </summary>
    /// <remarks>
    /// Implements two interfaces with overlapping method declarations, the only difference in the 
    /// main being the return types, that of the interface itself for the fluent api, so both 
    /// interfaces are implemented explicitly.
    /// Each explicit interface method simply calls a private routine of the same name and with 
    /// the parameters and supplies the return value via an implicit cast from the class instance 
    /// to the interface instance.
    /// There are two 'build' methods that conclude the fluent chain, one for each interface:
    /// <c>.ToArray();</c> and <c>.ToString();</c>.
    /// </remarks>
    internal partial class Serialization : 
        IOptionSerialization,
        ISerialization
    {
        #region Fields

        private readonly List<BaseSerializationData> serializations = new();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="Options"/> that the current instance is using.
        /// </summary>
        public Options Options { get; }

        #endregion

        #region Constructors

        internal Serialization(Options options) => this.Options = options;

        #endregion

        #region IOptionSerialization methods

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.GlobalConfig(string key, string value) => 
            GlobalConfig(key, value);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.GlobalConfig(Dictionary<string, string> globalConfiguration) => 
            GlobalConfig(globalConfiguration);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypeConfig<TObject>(string key, string value) =>
            TypeConfig<TObject>(key, value);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypeConfig(Type type, string key, string value) =>
            TypeConfig(type, key, value);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypeConfig(string typeName, string key, string value) => 
            TypeConfig(typeName, key, value);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypeConfig<TObject>(Dictionary<string, string> typeConfiguration)
            where TObject : class => TypeConfig<TObject>(typeConfiguration);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypeConfig(Type type, Dictionary<string, string> typeConfiguration) =>
            TypeConfig(type, typeConfiguration);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypeConfig(string typeName, Dictionary<string, string> typeConfiguration) =>
            TypeConfig(typeName, typeConfiguration);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.BlankLine(int number) => BlankLine(number);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypeConversion(Dictionary<string, string> typeConversion) =>
            TypeConversion(typeConversion);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.ValueConversion(Dictionary<string, string> valueConversion) => 
            ValueConversion(valueConversion);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.Comment(string commentPrefix, params string[] comments) => 
            Comment(commentPrefix, comments);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypedMetadata<TMetadata, TObject>(IEnumerable<TMetadata> metadata) 
            where TMetadata : class
            where TObject : class => TypedMetadata<TMetadata, TObject>(metadata);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypedMetadata<TMetadata>(Type dataType, IEnumerable<TMetadata> metadata) =>
            TypedMetadata<TMetadata>(dataType, metadata);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypedMetadata<TMetadata>(string dataTypeName, IEnumerable<TMetadata> metadata) =>
            TypedMetadata(dataTypeName, metadata);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypelessMetadata<TObject>(string metadataPrefix, Dictionary<string, string> metadata) =>
            TypelessMetadata<TObject>(metadataPrefix, metadata);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypelessMetadata(Type dataType, string metadataPrefix, Dictionary<string, string> metadata) =>
            TypelessMetadata(dataType, metadataPrefix, metadata);

        /// <inheritdoc/>
        IOptionSerialization IOptionSerialization.TypelessMetadata(string dataTypeName, string metadataPrefix, Dictionary<string, string> metadata) => 
            TypelessMetadata(dataTypeName, metadataPrefix, metadata);

        /// <inheritdoc/>
        string[] IOptionSerialization.ToArray() => ToArray();

        #endregion

        #region ISerialization methods

        /// <inheritdoc/>
        ISerialization ISerialization.GlobalConfig(string key, string value) => 
            GlobalConfig(key, value);

        /// <inheritdoc/>
        ISerialization ISerialization.GlobalConfig(Dictionary<string, string> globalConfiguration) =>
            GlobalConfig(globalConfiguration);

        /// <inheritdoc/>
        ISerialization ISerialization.TypeConfig<TObject>(string key, string value) =>
            TypeConfig<TObject>(key, value);

        /// <inheritdoc/>
        ISerialization ISerialization.TypeConfig(Type type, string key, string value) => 
            TypeConfig(type, key, value);

        /// <inheritdoc/>
        ISerialization ISerialization.TypeConfg(string typeName, string key, string value) => 
            TypeConfig(typeName, key, value);

        /// <inheritdoc/>
        ISerialization ISerialization.TypeConfig<TObject>(Dictionary<string, string> typeConfiguration)
            where TObject : class => TypeConfig<TObject>(typeConfiguration);

        /// <inheritdoc/>
        ISerialization ISerialization.TypeConfig(Type type, Dictionary<string, string> typeConfiguration) =>
            TypeConfig(type, typeConfiguration);

        /// <inheritdoc/>
        ISerialization ISerialization.TypeConfig(string typeName, Dictionary<string, string> typeConfiguration) => 
            TypeConfig(typeName, typeConfiguration);

        /// <inheritdoc/>
        ISerialization ISerialization.BlankLine(int number) => BlankLine(number);

        /// <inheritdoc/>
        ISerialization ISerialization.TypeConversion(Dictionary<string, string> typeConversion) => 
            TypeConversion(typeConversion);

        /// <inheritdoc/>
        ISerialization ISerialization.ValueConversion(Dictionary<string, string> valueConversion) => 
            ValueConversion(valueConversion);

        /// <inheritdoc/>
        ISerialization ISerialization.Comment(string commentPrefix, params string[] comments) => 
            Comment(commentPrefix, comments);

        /// <inheritdoc/>
        ISerialization ISerialization.TypedMetadata<TMetadata, TObject>(IEnumerable<TMetadata> metadata) =>
            TypedMetadata<TMetadata, TObject>(metadata);

        /// <inheritdoc/>
        ISerialization ISerialization.TypedMetadata<TMetadata>(Type dataType, IEnumerable<TMetadata> metadata) =>
            TypedMetadata<TMetadata>(dataType, metadata);

        /// <inheritdoc/>
        ISerialization ISerialization.TypedMetadata<TMetadata>(string dataTypeName, IEnumerable<TMetadata> metadata) => 
            TypedMetadata<TMetadata>(dataTypeName, metadata);

        /// <inheritdoc/>
        ISerialization ISerialization.TypelessMetadata<TObject>(string metadataPrefix, Dictionary<string, string> metadata) => 
            TypelessMetadata<TObject>(metadataPrefix, metadata);

        /// <inheritdoc/>
        ISerialization ISerialization.TypelessMetadata(Type dataType, string metadataPrefix, Dictionary<string, string> metadata) =>
            TypelessMetadata(dataType, metadataPrefix, metadata);

        /// <inheritdoc/>
        ISerialization ISerialization.TypelessMetadata(string dataTypeName, string metadataPrefix, Dictionary<string, string> metadata) => 
            TypelessMetadata(dataTypeName, metadataPrefix, metadata);

        /// <inheritdoc/>
        ISerialization ISerialization.TypedData<Tobject>(IEnumerable<Tobject> data) => 
            TypedData(data);

        /// <inheritdoc/>
        ISerialization ISerialization.TypelessData(string typeName, string[] names, IEnumerable<string[]> values) => 
            TypelessData(typeName, names, values);

        /// <inheritdoc/>
        string ISerialization.ToString() =>
            string.Join(Environment.NewLine, ToArray(this.Options.OptionSerialize.ToArray()));

        #endregion

        #region Instance methods for interfaces

        private Serialization GlobalConfig(string key, string value) => 
            Add(new GlobalConfigData(new Dictionary<string, string> { [key] = value, }));

        private Serialization GlobalConfig(Dictionary<string, string> globalConfiguration) =>
            Add(new GlobalConfigData(globalConfiguration));

        private Serialization TypeConfig<TObject>(string key, string value)
            where TObject : class => TypeConfig(typeof(TObject), key, value);

        private Serialization TypeConfig(Type type, string key, string value)=>
            TypeConfig(type.Name, key, value);

        private Serialization TypeConfig(string typeName, string key, string value) =>
            Add(new TypedConfigData(typeName, new Dictionary<string, string> { [key] = value, }));

        private Serialization TypeConfig<TObject>(Dictionary<string, string> typeConfiguration)
            where TObject : class => TypeConfig(typeof(TObject), typeConfiguration);

        private Serialization TypeConfig(Type type, Dictionary<string, string> typeConfiguration) =>
            TypeConfig(type.Name, typeConfiguration);

        private Serialization TypeConfig(string typeName, Dictionary<string, string> typeConfiguration) => 
            Add(new TypedConfigData(typeName, typeConfiguration));

        private Serialization BlankLine(int number)=> Add(new BlankLinesData(number));

        private Serialization TypeConversion(Dictionary<string, string> typeConversion) => 
            Add(new TypeConversionData(typeConversion));

        private Serialization ValueConversion(Dictionary<string, string> valueConversion) => 
            Add(new ValueConversionData(valueConversion));

        private Serialization Comment(string commentPrefix, string[] comments) => 
            Add(new CommentData(commentPrefix, comments));

        private Serialization TypedMetadata<TMetadata, TObject>(IEnumerable<TMetadata> metadata)
            where TMetadata : class
            where TObject : class => TypedMetadata<TMetadata>(GetTypeName<TObject>(), metadata);

        private Serialization TypedMetadata<TMetadata>(Type dataType, IEnumerable<TMetadata> metadata) 
            where TMetadata : class => TypedMetadata<TMetadata>(dataType.GetTypeName(), metadata);

        private Serialization TypedMetadata<TMetadata>(string dataTypeName, IEnumerable<TMetadata> metadata)
            where TMetadata : class => Add(new TypedMetadataData<TMetadata>(new SerializationFactory(this), dataTypeName, metadata));

        private Serialization TypelessMetadata<TObject>(string metadataPrefix, Dictionary<string, string> metadata)
            where TObject : class => TypelessMetadata(GetTypeName<TObject>(), metadataPrefix, metadata);

        private Serialization TypelessMetadata(Type dataType, string metadataPrefix, Dictionary<string, string> metadata) =>
            TypelessMetadata(dataType.GetTypeName(), metadataPrefix, metadata);

        private Serialization TypelessMetadata(string dataTypeName, string metadataPrefix, Dictionary<string, string> metadata) =>
            Add(new TypelessMetadataData(new SerializationFactory(this), dataTypeName, metadataPrefix, metadata));

        private Serialization TypedData<TObject>(IEnumerable<TObject> data) where TObject : class =>
            TypedData<TObject>(data, new SerializationFactory(this), GetTypeName<TObject>());

        private Serialization TypedData<TObject>(IEnumerable<TObject> data, SerializationFactory factory, string objectTypeName)
            where TObject : class =>
            Add(new AttributeMetadataData(factory, objectTypeName, typeof(TObject)),
                new TypedObjectData<TObject>(factory, objectTypeName, data));

        private Serialization TypelessData(string typeName, string[] names, IEnumerable<string[]> values) =>
            Add(new TypelessObjectData(new SerializationFactory(this), typeName, names, values));

        #endregion

        #region Support routines

        /// <summary>
        /// Adds the specified <paramref name="data"/> to the <seealso cref="serializations"/> 
        /// list and returns the current instance.
        /// </summary>
        /// <param name="data">A <see cref="BaseSerializationData"/> object.</param>
        /// <returns>The current instance which can be passed back by the calling routine and so enable fluent chaining.</returns>
        protected Serialization Add(params BaseSerializationData[] data)
        {
            this.serializations.AddRange(data);
            return this;
        }

        /// <summary>
        /// Gets the type name from either the <typeparamref name="T"/> or its 
        /// <see cref="CsvConverterClassAttribute"/> if any.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A <see cref="string"/>.</returns>
        private static string GetTypeName<T>() => typeof(T).GetTypeName();

        /// <summary>
        /// Serialize the <seealso cref="serializations"/> preceeded by the specified <paramref name="preceedingData"/> 
        /// data to a <see cref="string[]"/>.
        /// </summary>
        /// <param name="preceedingData">A <see cref="string[]"/> containing data that is to be included before the <seealso cref="serializations"/> data.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] ToArray(string[] preceedingData) =>
            new List<string[]> { preceedingData, this.ToArray(), }
                .SelectMany(items => items)
                .ToArray();

        /// <summary>
        /// Serializes the <seealso cref="serializations"/> to a <see cref="string[]"/>.
        /// </summary>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] ToArray() =>
            this.serializations
                .Select(serialization => serialization.Serialize())
                .SelectMany(lines => lines)
                .ToArray();

        #endregion

        #region Nested classes

        /// <summary>
        /// Sealed factory class to allow the dynamic provision of data from a <see cref="Serialization"/>
        /// object.
        /// </summary>
        internal sealed class SerializationFactory
        {
            private readonly Serialization serialization;

            /// <summary>
            /// Gets the <see cref="Options"/> associated with the current instance.
            /// </summary>
            internal Options Options => this.serialization.Options;

            /// <summary>
            /// Gets the <see cref="Serialization"/> instances.
            /// </summary>
            private IEnumerable<BaseSerializationData> Serializations => this.serialization.serializations;

            public SerializationFactory(Serialization serialization) => this.serialization = serialization;

            /// <summary>
            /// Gets the Property Prefix for the specified <paramref name="typeName"/>.
            /// </summary>
            /// <param name="typeName">A <see cref="string"/> that contains the type name.</param>
            /// <returns>A <see cref="string"/>.</returns>
            public string GetPropertyPrefix(string typeName) =>
                ConfigHelper.GetPropertyPrefix(
                    GetGlobalConfig(),
                    GetTypedConfig(),
                    this.Options,
                    typeName);

            /// <summary>
            /// Gets the Value Prefix for the specified <paramref name="typeName"/>.
            /// </summary>
            /// <param name="typeName">A <see cref="string"/> that contains the type name.</param>
            /// <returns>A <see cref="string"/>.</returns>
            public string GetvaluePrefix(string typeName) =>
                ConfigHelper.GetValuePrefix(
                    GetGlobalConfig(),
                    GetTypedConfig(),
                    this.Options,
                    typeName);

            /// <summary>
            /// Gets the Global Config data from the <seealso cref="serialization"/> object.
            /// </summary>
            /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="IGlobalConfig"/> objects.</returns>
            private IEnumerable<IGlobalConfig> GetGlobalConfig() =>
                this.Serializations
                    .Select(s => s as GlobalConfigData)
                    .NotNull()
                    .SelectMany(item => item);

            /// <summary>
            /// Gets the Typed Config data from the <seealso cref="serialization"/> object.
            /// </summary>
            /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ITypedConfig"/> objects.</returns>
            private IEnumerable<ITypedConfig> GetTypedConfig() =>
                this.Serializations
                    .Select(s => s as TypedConfigData)
                    .NotNull()
                    .SelectMany(item => item);
        }

        #endregion
    }
}
