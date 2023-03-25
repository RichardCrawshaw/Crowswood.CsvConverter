using System.Reflection;
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
        string ISerialization.ToString()
        {
            // Do pre-serialization adjustments.
            PreSerializationAdjustments();

            // Finally do the serialization.
            var result =
                string.Join(Environment.NewLine,
                            ToArray(this.Options.OptionSerialize.ToArray()));
            return result;
        }

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
            where TMetadata : class => TypedMetadata<TMetadata>(GetTypeName(dataType), metadata);

        private Serialization TypedMetadata<TMetadata>(string dataTypeName, IEnumerable<TMetadata> metadata)
            where TMetadata : class => Add(new TypedMetadataData<TMetadata>(new SerializationFactory(this), dataTypeName, metadata));

        private Serialization TypelessMetadata<TObject>(string metadataPrefix, Dictionary<string, string> metadata)
            where TObject : class => TypelessMetadata(GetTypeName<TObject>(), metadataPrefix, metadata);

        private Serialization TypelessMetadata(Type dataType, string metadataPrefix, Dictionary<string, string> metadata) =>
            TypelessMetadata(GetTypeName(dataType), metadataPrefix, metadata);

        private Serialization TypelessMetadata(string dataTypeName, string metadataPrefix, Dictionary<string, string> metadata) =>
            Add(new TypelessMetadataData(new SerializationFactory(this), dataTypeName, metadataPrefix, metadata));

        private Serialization TypedData<TObject>(IEnumerable<TObject> data) where TObject : class =>
            Add(new TypedObjectData<TObject>(new SerializationFactory(this), GetTypeName<TObject>(), data));

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
        protected Serialization Add(BaseSerializationData data)
        {
            this.serializations.Add(data);
            return this;
        }

        /// <summary>
        /// Add into the <seealso cref="serializations"/> new <see cref="TypedMetadataData{TMetadata}"/> 
        /// as defined in the <paramref name="objectDataMetadataTypes"/> using the specified 
        /// <paramref name="typedObjectData"/>.
        /// </summary>
        /// <param name="objectDataMetadataTypes">A <see cref="List{T}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="Type"/> and <see cref="List{T}"/> of <see cref="Type"/> that contains the object data and metadata types that define the <see cref="TypedMetadataData{TMetadata}"/> instances to insert.</param>
        /// <param name="typedObjectData">An <see cref="IEnumerable{T}"/> of <see cref="TypedObjectData"/> containing the existing typed object data serializations.</param>
        private void AddMetadataSerializations(List<(Type ObjectType, List<Type> MetadataTypes)> objectDataMetadataTypes, IEnumerable<TypedObjectData> typedObjectData)=>
            objectDataMetadataTypes
                .ForEach(item =>
                    AddMetadataSerializations(typedObjectData, item.ObjectType, item.MetadataTypes));

        /// <summary>
        /// Add into the <seealso cref="serializations"/> before the <see cref="TypedObjectData{TObject}"/> 
        /// serialization for the specified <paramref name="objectType"/> from the specified 
        /// <paramref name="typedObjectData"/> a set of <see cref="TypedMetadataData{TMetadata}"/>
        /// for the <paramref name="metadataTypes"/>.
        /// </summary>
        /// <param name="typedObjectData">An <see cref="IEnumerable{T}"/> of <see cref="TypedObjectData"/> containing the existing typed object data serializations.</param>
        /// <param name="objectType">A <see cref="Type"/> indicating the type of the object data.</param>
        /// <param name="metadataTypes">A <see cref="List{T}"/> of <see cref="Type"/> containing the types of types of the metadata serializations.</param>
        private void AddMetadataSerializations(IEnumerable<TypedObjectData> typedObjectData, Type objectType, IEnumerable<Type> metadataTypes)
        {
            // Find the position of the object data serialization

            var objectDataSerialization =
                typedObjectData
                    .FirstOrDefault(item => item.Type == objectType);
            if (objectDataSerialization is null) 
                return;

            var index = this.serializations.IndexOf(objectDataSerialization);
            if (index == -1) 
                return;

            var dataTypeName = GetTypeName(objectType);

            // Now retrieve the metadata and filter by the the definitions in Options.
            var metadata =
                objectType.GetAttributes()
                    .Where(item => this.Options.OptionMetadata.Any(om => om.Type == item.GetType()))
                    .ToList();

            // Create the required metadata serialization objects.
            var metadataSerializations =
                metadataTypes
                    .Select(metadataType =>
                        TypedMetadataData.Create(metadataType,
                                                 this,
                                                 dataTypeName,
                                                 metadata))
                    .ToList();
            this.serializations.InsertRange(index, metadataSerializations);
        }

        /// <summary>
        /// Determines and returns a list of the object type and metadata type combinations that 
        /// are missing from the <seealso cref="serializations"/> using the specified <paramref name="typedObjectData"/> 
        /// and <paramref name="typedMetadataData"/>.
        /// </summary>
        /// <param name="typedObjectData">A <see cref="List{T}"/> of <see cref="TypedObjectData"/> instances.</param>
        /// <param name="typedMetadataData">A <see cref="List{T}"/> of <see cref="TypedMetadataData"/> instances.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="Type"/> and <see cref="List{T}"/> of <see cref="Type"/>.</returns>
        private List<(Type ObjectType, List<Type> MetadataTypes)> GetObjectTypeMetadataTypeCombinations(IEnumerable<TypedObjectData> typedObjectData, IEnumerable<TypedMetadataData> typedMetadataData)
        {
            var combinations =
                typedObjectData
                    .Select(item => item.Type)
                    .Select(type => new
                    {
                        ObjectType = type,
                        MetadataTypes =
                            type.GetAttributes()
                                .GetAttributeTypes()
                                .Where(attrType => this.Options.OptionMetadata.Any(om => om.Type == attrType))
                                .ToArray(),
                    })
                    .Select(item =>
                        item.MetadataTypes
                            .Select(metadataType => new { item.ObjectType, MetadataType = metadataType, }))
                    .SelectMany(item => item)
                    .Select(pair => (pair.ObjectType, pair.MetadataType))
                    .ToList();

            combinations.
                RemoveAll(item =>
                    typedMetadataData
                        .Any(serialization =>
                            serialization.MetadataType == item.ObjectType &&
                            serialization.MetadataTypeName == item.MetadataType.Name));

            var results =
                combinations
                    .Select(item => item.ObjectType)
                    .Distinct()
                    .Select(objectType => new
                    {
                        ObjectType = objectType,
                        MetadataTypes =
                            combinations
                                .Where(item => item.ObjectType == objectType)
                                .Select(item => item.MetadataType)
                                .ToList()
                    })
                    .Select(item => (item.ObjectType, item.MetadataTypes))
                    .ToList();

            return results;
        }

        /// <summary>
        /// Gets the type name from either the <typeparamref name="T"/> or its 
        /// <see cref="CsvConverterClassAttribute"/> if any.
        /// </summary>
        /// <typeparam name="T">The type of object.</typeparam>
        /// <returns>A <see cref="string"/>.</returns>
        private static string GetTypeName<T>() => GetTypeName(typeof(T));

        /// <summary>
        /// Gets the type name from either the specified <paramref name="type"/> or its 
        /// <see cref="CsvConverterClassAttribute"/> if any.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="string"/>.</returns>
        private static string GetTypeName(Type type) =>
            type.GetCustomAttribute<CsvConverterClassAttribute>()?.Name ??
            type.Name;

        /// <summary>
        /// Perform pre-serialization adjustments.
        /// </summary>
        /// <remarks>
        /// The adjustments include adding in any <see cref="TypedMetadataData{TMetadata}"/> 
        /// instances where the <code>ObjectData</code> type has attributes that are defined in 
        /// <seealso cref="Options.OptionMetadata"/> but no corresponding entry in <seealso cref="serializations"/>
        /// exists.
        /// </remarks>
        private void PreSerializationAdjustments()
        {
            // This is a list of all the typed metadata instances.
            var typedMetadataData =
                this.serializations.Get<TypedMetadataData>()
                    .ToList();

            // This is a list of all the typed object data instances.
            var typedObjectData =
                this.serializations.Get<TypedObjectData>()
                    .ToList();

            // There should be a TypedMetadataData serialization for each `typedObjectData`
            // instance that has an attribute with a type that is included in the option metadata.
            // Create a virtual list of all the expected metadata serializations and remove all
            // those that already exist; this will leave just those that are missing.
            var objectDataMetadataTypes =
                GetObjectTypeMetadataTypeCombinations(typedObjectData, typedMetadataData);

            // So for each object type insert before it appropriate serializations for the metadata 
            // types from `objectDataMetadataTypes`.
            AddMetadataSerializations(objectDataMetadataTypes, typedObjectData);
        }

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

            private IEnumerable<BaseSerializationData> Serializations => this.serialization.serializations;

            public SerializationFactory(Serialization serialization) => this.serialization = serialization;

            /// <summary>
            /// Gets the Metadata Type Names.
            /// </summary>
            /// <typeparam name="T">A <see cref="Type"/> to use to restrict to either typed or typeless metadata.</typeparam>
            /// <returns>A <see cref="string[]"/>.</returns>
            public string[] GetMetadataTypeNames<T>() where T : BaseMetadataData =>
                this.Serializations
                    .Where(serialization => serialization.GetType().IsAssignableTo(typeof(T)))
                    .Select(serialization => serialization as T)
                    .NotNull()
                    .Select(metadataSerialization => metadataSerialization.MetadataTypeName)
                    .ToArray();

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
