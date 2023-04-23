using Crowswood.CsvConverter.Deserializations;
using Crowswood.CsvConverter.Exceptions;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Interfaces;
using Crowswood.CsvConverter.Model;
using Crowswood.CsvConverter.UserConfig;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An internal class to manage the deserialization of data using a fluent interface.
    /// </summary>
    internal class Deserialization :
        IDeserialization,
        IDeserializationData
    {
        #region Fields

        private readonly string text;
        private readonly List<string> lines;

        private readonly Lazy<List<GlobalConfig>> lazyGlobalConfig;
        private readonly Lazy<List<TypedConfig>> lazyTypedConfig;
        private readonly Lazy<List<ConversionType>> lazyConversionTypes;
        private readonly Lazy<List<ConversionValue>> lazyConversionValues;

        private readonly List<BaseObjectData> objectData = new();

        // Metadata is a function of the object type that it relates to; so it is accessed via
        // that data object, rather than having a field to hold it in its own right.

        #endregion

        #region Properties

        public Options Options { get; }

        internal GlobalConfig[] GlobalConfig => this.lazyGlobalConfig.Value.ToArray();
        internal TypedConfig[] TypedConfig => this.lazyTypedConfig.Value.ToArray();
        internal ConversionType[] TypeConversions => this.lazyConversionTypes.Value.ToArray();
        internal ConversionValue[] ValueConversions => this.lazyConversionValues.Value.ToArray();

        public Dictionary<string, List<object>> Metadata { get; } = new();

        #endregion

        #region Constructors

        internal Deserialization(Options options, string text)
        {
            this.Options = options;
            this.text = text;
            this.lines = ConverterHelper.SplitLines(text, options);

            this.lazyGlobalConfig = new(() => DeserializeGlobalConfig(this));
            this.lazyTypedConfig = new(() => DeserializeTypedConfig(this));
            this.lazyConversionTypes = new(() => DeserializeTypeCoversion(this));
            this.lazyConversionValues = new(() => DeserializeValueConversion(this));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Return an <see cref="IDeserialization"/> object based on the current instance.
        /// </summary>
        /// <returns>An <see cref="IDeserialization"/> object.</returns>
        public IDeserialization Initialise() => this;

        #endregion

        #region IDeserialization methods

        /// <inheritdoc/>
        IDeserialization IDeserialization.Data<TObject>() => 
            RegisterObjectData<TObject>();

        /// <inheritdoc/>
        IDeserialization IDeserialization.Data(params Type[] objectTypes) =>
            objectTypes
                .Select(objectType => RegisterObjectData(objectType))
                .Distinct()
                .FirstOrDefault() ?? this;

        /// <inheritdoc/>
        IDeserialization IDeserialization.Data(params string[] objectTypeNames) =>
            objectTypeNames
                .Select(objectTypeName => RegisterObjectData(objectTypeName))
                .Distinct()
                .FirstOrDefault() ?? this;

        /// <inheritdoc/>
        List<string> IDeserialization.GetTypeNames() => GetTypeNames();

        /// <inheritdoc/>
        List<Type> IDeserialization.GetTypes(params Type[] types) => GetTypes(types);

        /// <inheritdoc/>
        IDeserializationData IDeserialization.Deserialize()
        {
            foreach (var deserializer in this.objectData)
                deserializer.Deserialize();
            return this;
        }

        #endregion

        #region IDeserializationData methods

        /// <inheritdoc/>
        List<TObject> IDeserializationData.GetData<TObject>() => GetObjectData<TObject>();

        /// <inheritdoc/>
        List<object> IDeserializationData.GetData(Type objectType) => GetObjectData(objectType);

        /// <inheritdoc/>
        List<object> IDeserializationData.GetData<TMetadata>(Type objectType, out List<TMetadata> metadata)
        {
            var objectTypeName = objectType.GetTypeName();
            metadata = GetMetadata<TMetadata>(objectTypeName);
            metadata.AddRange(
                GetMetadata(objectTypeName, typeof(TMetadata))
                    .Cast<TMetadata>());
            return GetObjectData(objectType);
        }

        /// <inheritdoc/>
        (string[] Names, IEnumerable<string[]> Values) IDeserializationData.GetData(string objectTypeName) =>
            GetObjectData(objectTypeName);

        /// <inheritdoc/>
        List<TObject> IDeserializationData.GetData<TObject, TMetadata>(
            out List<TMetadata> metadata)
        {
            var objectTypeName = typeof(TObject).GetTypeName();
            metadata = GetMetadata<TMetadata>(objectTypeName);
            metadata.AddRange(
                GetMetadata(objectTypeName, typeof(TMetadata))
                    .Cast<TMetadata>());
            return GetObjectData<TObject>();
        }

        /// <inheritdoc/>
        List<object> IDeserializationData.GetData(
            Type objectType,
            string metadataTypeName,
            out (string[] Names, IEnumerable<string[]> Values) metadata)
        {
            var objectTypeName = objectType.GetTypeName();
            metadata = GetMetadata(metadataTypeName, objectTypeName);
            return GetObjectData(objectType);
        }

        /// <inheritdoc/>
        (string[] Names, IEnumerable<string[]> Values) IDeserializationData.GetData(
            string objectTypeName, string metadataTypeName,
            out (string[] Names, IEnumerable<string[]> Values) metadata)
        {
            metadata = GetMetadata(metadataTypeName, objectTypeName);
            return GetObjectData(objectTypeName);
        }

        /// <inheritdoc/>
        List<TObject> IDeserializationData.GetData<TObject>(out List<object> metadata)
        {
            metadata = GetAllTypedMetadata(typeof(TObject).GetTypeName());
            return GetObjectData<TObject>();
        }

        /// <inheritdoc/>
        (string[] Names, IEnumerable<string[]> Values) IDeserializationData.GetData<TMetadata>(
            string objectTypeName,
            out List<TMetadata> metadata)
        {
            metadata = GetMetadata<TMetadata>(objectTypeName);
            metadata.AddRange(
                GetMetadata(objectTypeName, typeof(TMetadata))
                    .Cast<TMetadata>());
            return GetObjectData(objectTypeName);
        }

        /// <inheritdoc/>
        List<TObject> IDeserializationData.GetData<TObject>(
            string metadataTypeName,
            out (string[] Names, IEnumerable<string[]> Values) metadata)
        {
            var objectTypeName = typeof(TObject).GetTypeName();
            metadata = GetMetadata(metadataTypeName, objectTypeName);
            return GetObjectData<TObject>();
        }

        /// <inheritdoc/>
        List<Type> IDeserializationData.GetObjectTypes() => 
            this.objectData
                .Select(od => od as TypedObjectData)
                .NotNull()
                .Select(tod => tod.Type)
                .Distinct()
                .ToList();

        /// <inheritdoc/>
        List<string> IDeserializationData.GetObjectTypeNames() => 
            this.objectData
                .Select(od => od as TypelessObjectData)
                .NotNull()
                .Select(tlod => tlod.ObjectTypeName)
                .Distinct()
                .ToList();

        #endregion

        #region Instance methods for interfaces

        /// <summary>
        /// Deserializes and returns the global configuration using the specified 
        /// <paramref name="deserialization"/>.
        /// </summary>
        /// <param name="deserialization">A <see cref="Deserialization"/> instance.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="GlobalConfig"/>.</returns>
        private static List<GlobalConfig> DeserializeGlobalConfig(Deserialization deserialization)
        {
            var deserializer = new GlobalConfigData(new DeserializationFactory(deserialization));
            deserializer.Deserialize();
            return deserializer.GlobalConfig.ToList();
        }

        /// <summary>
        /// Deserializes and returns the typed configuration using the specified 
        /// <paramref name="deserialization"/>.
        /// </summary>
        /// <param name="deserialization">A <see cref="Deserialization"/> instance.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="TypedConfig"/>.</returns>
        private static List<TypedConfig> DeserializeTypedConfig(Deserialization deserialization)
        {
            var deserializer = new TypedConfigData(new DeserializationFactory(deserialization));
            deserializer.Deserialize();
            return deserializer.TypedConfig.ToList();
        }

        /// <summary>
        /// Deserializes and returns the conversion types using the specified 
        /// <paramref name="deserialization"/>.
        /// </summary>
        /// <param name="deserialization">A <see cref="Deserialization"/> instance.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ConversionType"/>.</returns>
        private static List<ConversionType> DeserializeTypeCoversion(Deserialization deserialization)
        {
            var deserializer = new TypeConversionData(new DeserializationFactory(deserialization));
            deserializer.Deserialize();
            return deserializer.TypedConversion.ToList();
        }

        /// <summary>
        /// Deserializes and returns the conversion values using the specified 
        /// <paramref name="deserialization"/>.
        /// </summary>
        /// <param name="deserialization">A <see cref="Deserialization"/> instance.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="ConversionValue"/>.</returns>
        private static List<ConversionValue> DeserializeValueConversion(Deserialization deserialization)
        {
            var deserializer = new ValueConversionData(new DeserializationFactory(deserialization));
            deserializer.Deserialize();
            return deserializer.ValueConversion.ToList();
        }

        /// <summary>
        /// Gets all the typed metadata for the specified <paramref name="objectTypeName"/>.
        /// </summary>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data type of the object.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="object"/>.</returns>
        /// <remarks>
        /// If <paramref name="objectTypeName"/> has no metadata then an empty <see cref="List{T}"/> 
        /// of <see cref="object"/> will be returned.
        /// </remarks>
        private List<object> GetAllTypedMetadata(string objectTypeName) =>
            this.objectData
                .Where(od => od.ObjectTypeName == objectTypeName)
                .Select(od => od.Metadata)
                .SelectMany(md => md)
                .Select(md => md as TypedMetadataData)
                .NotNull()
                .Select(md => md.GetMetadata())
                .SelectMany(md => md)
                .ToList();

        /// <summary>
        /// Gets all the metadata of <typeparamref name="TMetadata"/> for the specified 
        /// <paramref name="objectTypeName"/>.
        /// </summary>
        /// <typeparam name="TMetadata">The <see cref="Type"/> of the metadata.</typeparam>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data type of the object.</param>
        /// <returns>A <see cref="List{T}"/> of <typeparamref name="TMetadata"/>.</returns>
        /// <remarks>
        /// If <paramref name="objectTypeName"/> has no <typeparamref name="TMetadata"/> metadata 
        /// then an empty <see cref="List{T}"/> of <typeparamref name="TMetadata"/> will be 
        /// returned.
        /// </remarks>
        private List<TMetadata> GetMetadata<TMetadata>(string objectTypeName)
            where TMetadata : class, new() =>
            this.objectData
                .Where(od => od.ObjectTypeName == objectTypeName)
                .Select(od => od.Metadata)
                .SelectMany(md => md)
                .Select(md => md as TypedMetadataData<TMetadata>)
                .NotNull()
                .Select(md => md.GetMetadata())
                .SelectMany(md => md)
                .ToList();

        /// <summary>
        /// Gets all the metadata of <paramref name="metadataType"/> for the specified 
        /// <paramref name="objectTypeName"/>.
        /// </summary>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data type of the object.</param>
        /// <param name="metadataType">A <see cref="Type"/> that indicates the type of the metadata.</param>
        /// <returns>A <see cref="List{T}"/> of <typeparamref name="TMetadata"/>.</returns>
        /// <remarks>
        /// If <paramref name="objectTypeName"/> has no <typeparamref name="TMetadata"/> metadata 
        /// then an empty <see cref="List{T}"/> of <typeparamref name="TMetadata"/> will be 
        /// returned.
        /// </remarks>
        private List<object> GetMetadata(string objectTypeName, Type metadataType) =>
            this.objectData
                .Where(od => od.ObjectTypeName == objectTypeName)
                .Select(od => od.Metadata)
                .SelectMany(md => md)
                .Select(md => md as TypedMetadataData)
                .NotNull()
                .Where(tmd => tmd.MetadataType == metadataType)
                .Select(tmd=> tmd.GetMetadata())
                .SelectMany(md => md)
                .ToList();

        /// <summary>
        /// Gets all of the typeless metadata that has the specified <paramref name="metadataTypeName"/> 
        /// for the specified <paramref name="objectTypeName"/>.
        /// </summary>
        /// <param name="metadataTypeName">A <see cref="string"/> containing the name of the data type of the metadata.</param>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data type of the object.</param>
        /// <returns>A tuple of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.</returns>
        /// <remarks>
        /// If the <paramref name="objectTypeName"/> has no <paramref name="metadataTypeName"/> 
        /// metadata then a tuple containing an <seealso cref="Array.Empty{T}"/> <see cref="string"/> 
        /// and <seealso cref="Enumerable.Empty{TResult}"/> <see cref="string[]"/> will be 
        /// returned.
        /// </remarks>
        private (string[] Names, IEnumerable<string[]> Values) GetMetadata(
            string metadataTypeName, string objectTypeName) =>
            this.objectData
                .Where(od => od.ObjectTypeName == objectTypeName)
                .Select(od => od.Metadata)
                .SelectMany(md => md)
                .Select(md => md as TypelessMetadataData)
                .NotNull()
                .Where(tmd => tmd.MetadataTypeName == metadataTypeName)
                .Select(tmd => tmd.GetMetadata())
                .GroupBy(metadata => metadata.Names)
                .Select(items => new
                {
                    Names = items.Key,
                    Values = items.SelectMany(values => values.Values),
                })
                .Select(items => (items.Names, items.Values))
                .FirstOrDefault();

        /// <summary>
        /// Gets the object data that has a type of <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">The Type of the object data.</typeparam>
        /// <returns>A <see cref="List{T}"/> of <typeparamref name="TObject"/>.</returns>
        /// <exception cref="InvalidOperationException">If <typeparamref name="TObject"/> is not a deserialized object type.</exception>
        private List<TObject> GetObjectData<TObject>() where TObject : class, new() =>
            this.objectData
                .Select(od => od as TypedObjectData<TObject>)
                .NotNull()
                .Select(tod => tod.GetData().ToList())
                .FirstOrDefault() ??
            throw new InvalidOperationException(
                $"The object type '{typeof(TObject).Name}' is not part of the deserialized data.");

        /// <summary>
        /// Gets the object data that has an object type of <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">A <see cref="Type"/> containing the type of the object data.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="object"/>.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="objectType"/> is not a deserialized object type.</exception>
        private List<object> GetObjectData(Type objectType) =>
            this.objectData
                .Select(od => od as TypedObjectData)
                .NotNull()
                .Where(tod => tod.Type == objectType)
                .Select(tod => tod.GetData().ToList())
                .FirstOrDefault() ??
            this.objectData
                .Select(od => od as ObjectData)
                .NotNull()
                .Where(od => od.ObjectTypeName == objectType.GetTypeName())
                .Select(od => new TypedObjectData(od, objectType))
                .Select(tod => tod.GetData().ToList())
                .FirstOrDefault() ??
            throw new NoObjectDataException(objectType);

        /// <summary>
        /// Gets the typeless object data that has an data type name of <paramref name="objectTypeName"/>.
        /// </summary>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the data type of the object.</param>
        /// <returns>A tuple of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="objectTypeName"/> is not a deserialized object data type name.</exception>
        private (string[] Names, IEnumerable<string[]> Values) GetObjectData(string objectTypeName) =>
            this.objectData
                .Select(od => od as TypelessObjectData)
                .NotNull()
                .Where(tlod => tlod.ObjectTypeName == objectTypeName)
                .FirstOrDefault()?.GetData() ??
            this.objectData
                .Select(od => od as ObjectData)
                .NotNull()
                .Where(od => od.ObjectTypeName == objectTypeName)
                .Select(od => new TypelessObjectData(od))
                .FirstOrDefault()?.GetData() ??
            throw new NoObjectDataException(objectTypeName, null);

        /// <summary>
        /// Gets all the items that have one of the specified <paramref name="prefixes"/>.
        /// </summary>
        /// <param name="prefixes">A <see cref="string[]"/> that contains the prefixes to filter by.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        private IEnumerable<string[]> GetItems(string[] prefixes) =>
            ConverterHelper.GetItems(this.lines,
                                     rejoinSplitQuotes: false,
                                     trimItems: true,
                                     typeName: null,
                                     prefixes);

        /// <summary>
        /// Gets all the Property Prefixes.
        /// </summary>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] GetPropertyPrefixes()=>
            ConfigHelper.GetPropertyPrefixes(this.lazyGlobalConfig.Value,
                                             this.lazyTypedConfig.Value,
                                             this.Options);

        /// <summary>
        /// Retrieves a list of the object type names that are contained in the text.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        private List<string> GetTypeNames() => GetTypeNames(GetPropertyPrefixes());

        /// <summary>
        /// Retrieves a list of the object type names that are contained in the text.
        /// </summary>
        /// <param name="prefixes">A <see cref="string[]"/> that contains the prefixes used to identify the properties.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        private List<string> GetTypeNames(string[] prefixes)=>
            GetItems(prefixes)
                .Select(properties => properties[1])
                .Distinct()
                .ToList();

        /// <summary>
        /// Retrieves a list of the object types from the specified <paramref name="types"/> 
        /// that are contained in the text.
        /// </summary>
        /// <param name="types">The <see cref="Type[]"/> that contains the expected types.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Type"/>.</returns>
        private List<Type> GetTypes(Type[] types) =>
            GetTypeNames()
                .Select(typeName => types.Where(type => type.Name == typeName))
                .SelectMany(types => types)
                .ToList();

        /// <summary>
        /// Registers the specified <typeparamref name="TObject"/> to be deserialized.
        /// </summary>
        /// <typeparam name="TObject">The object type to deserialize.</typeparam>
        /// <returns>A <see cref="Deserialization"/> object to allow chaining.</returns>
        private Deserialization RegisterObjectData<TObject>() where TObject : class, new()
        {
            this.objectData.Add(new TypedObjectData<TObject>(new DeserializationFactory(this)));
            return this;
        }

        /// <summary>
        /// Registers the specified <paramref name="objectType"/> to be deserialized.
        /// </summary>
        /// <param name="objectType">A <see cref="Type"/> containing the object data type.</param>
        /// <returns>A <see cref="Deserialization"/> object to allow chaining.</returns>
        private Deserialization RegisterObjectData(Type objectType)
        {
            this.objectData.Add(new TypedObjectData(new DeserializationFactory(this), objectType));
            return this;
        }

        /// <summary>
        /// Registers the data type with the specified <paramref name="objectTypeName"/> to be 
        /// deserialized.
        /// </summary>
        /// <param name="objectTypeName">A <see cref="string"/> containing the name of the object data type.</param>
        /// <returns>A <see cref="Deserialization"/> object to allow chaining.</returns>
        private Deserialization RegisterObjectData(string objectTypeName)
        {
            this.objectData.Add(new ObjectData(new DeserializationFactory(this), objectTypeName));
            return this;
        }

        #endregion

        #region Nested classes

        internal sealed class DeserializationFactory
        {
            private readonly Deserialization deserialization;

            public Options Options => this.deserialization.Options;

            public List<string> Lines => this.deserialization.lines;

            public string Text => this.deserialization.text;

            public GlobalConfig[] GlobalConfig => this.deserialization.GlobalConfig;
            public TypedConfig[] TypedConfig => this.deserialization.TypedConfig;
            public ConversionType[] ConversionTypes => this.deserialization.TypeConversions;
            public ConversionValue[] ConversionValues => this.deserialization.ValueConversions;

            public DeserializationFactory(Deserialization deserialization) =>
                this.deserialization = deserialization;

            internal DeserializationFactory Clone() => new(this.deserialization);

            public BaseObjectData? GetObjectData(string objectDataTypeName) =>
                this.deserialization.objectData
                    .FirstOrDefault(objectData => objectData.ObjectTypeName == objectDataTypeName);
        }

        #endregion
    }
}
