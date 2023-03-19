using System.Reflection;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Interfaces;

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// An internal class to manage the serialization of data using a fluent interface.
    /// </summary>
    internal class Serialization :
        ISerialization
    {
        #region Fields

        private readonly List<BaseSerializationData> serializations = new();

        private readonly ConfigHandler configHandler;

        #endregion

        #region Properties

        public Converter Converter { get; }
        public Options Options { get; }

        #endregion

        #region Constructors

        internal Serialization(Converter converter, Options options)
        {
            this.Converter = converter;
            this.Options = options;

            this.configHandler = new ConfigHandler(this.Options);
        }

        #endregion

        #region Interface methods

        /// <inheritdoc/>
        public ISerialization GlobalConfig(Dictionary<string, string> globalConfiguration)
        {
            this.serializations.Add(
                new GlobalConfigData(globalConfiguration));
            return this;
        }

        /// <inheritdoc/>
        public ISerialization TypeConfig(string typeName, Dictionary<string, string> typeConfiguration)
        {
            this.serializations.Add(
                new TypedConfigData(typeName, typeConfiguration));
            return this;
        }

        /// <inheritdoc/>
        public ISerialization BlankLine(int number = 1)
        {
            this.serializations.Add(
                new BlankLinesData(number));
            return this;
        }

        /// <inheritdoc/>
        public ISerialization TypeConversion(Dictionary<string, string> typeConversion)
        {
            this.serializations.Add(
                new TypeConversionData(typeConversion));
            return this;
        }

        /// <inheritdoc/>
        public ISerialization ValueConversion(Dictionary<string, string> valueConversion)
        {
            this.serializations.Add(
                new ValueConversionData(valueConversion));
            return this;
        }

        /// <inheritdoc/>
        public ISerialization Comment(string commentPrefix, params string[] comments)
        {
            this.serializations.Add(
                new CommentData(commentPrefix, comments));
            return this;
        }

        /// <inheritdoc/>
        public ISerialization TypedMetadata<TMetadata>(string dataTypeName, IEnumerable<TMetadata> metadata)
            where TMetadata : class
        {
            var metadataType = typeof(TMetadata);
            var optionMetadata =
                this.Options.GetOptionMetadata(metadataType) ??
                throw new InvalidOperationException(
                    $"No Metadata definition in Options for '{metadataType.Name}'.");

            this.serializations.Add(
                new TypedMetadataData<TMetadata>(dataTypeName,
                                                 optionMetadata.Prefix,
                                                 optionMetadata.PropertyNames,
                                                 metadata));

            return this;
        }

        /// <inheritdoc/>
        public ISerialization TypelessMetadata(string dataTypeName, string metadataPrefix, Dictionary<string, string> metadata)
        {
            var optionMetadata =
                this.Options.GetOptionMetadata(metadataPrefix) ?? 
                throw new InvalidOperationException(
                    $"No Metadata definition in Options for a prefix of '{metadataPrefix}'.");

            this.serializations.Add(
                new TypelessMetadataData(dataTypeName,
                                         optionMetadata.Prefix,
                                         optionMetadata.PropertyNames,
                                         metadata));

            return this;
        }

        /// <inheritdoc/>
        public ISerialization TypedData<Tobject>(IEnumerable<Tobject> data)
            where Tobject : class
        {
            var type = typeof(Tobject);
            var typeName =
                type.GetCustomAttribute<CsvConverterClassAttribute>()?.Name ??
                type.Name;
            var propertyPrefix =
                this.configHandler.GetPropertyPrefix(typeName);
            var valuePrefix =
                this.configHandler.GetValuePrefix(typeName);

            this.serializations.Add(
                new TypedDataData<Tobject>(propertyPrefix, valuePrefix, typeName, data));
            return this;
        }

        /// <inheritdoc/>
        public ISerialization TypelessData(string typeName, string[] names, IEnumerable<string[]> values)
        {
            var propertyPrefix =
                this.configHandler.GetPropertyPrefix(typeName);
            var valuePrefix =
                this.configHandler.GetValuePrefix(typeName);

            this.serializations.Add(new TypelessDataData(propertyPrefix, valuePrefix, typeName, names, values));
            return this;
        }

        /// <inheritdoc/>
        public string Serialize()
        {
            List<string> list = new();

            foreach (var serialization in this.serializations)
                list.AddRange(serialization.Serialize());

            var text = string.Join(Environment.NewLine, list);
            return text;
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// The abstract base class for holding data to be serialized.
        /// </summary>
        private abstract class BaseSerializationData
        {
            /// <summary>
            /// Serialize the data held in the current instance.
            /// </summary>
            /// <returns>A <see cref="string"/> containing the result of the serialization.</returns>
            public abstract string[] Serialize();
        }

        /// <summary>
        /// A sealed class for holding blank line data.
        /// </summary>
        private sealed class BlankLinesData : BaseSerializationData
        {
            private readonly int number;

            public BlankLinesData(int number) => this.number = number;

            /// <inheritdoc/>
            public override string[] Serialize() =>
                Enumerable
                    .Range(0, this.number)
                    .Select(_ => string.Empty)
                    .ToArray();
        }

        /// <summary>
        /// A sealed class for serializing comments.
        /// </summary>
        private sealed class CommentData : BaseSerializationData
        {
            private readonly string prefix;
            private readonly string[] comments;

            public CommentData(string prefix, params string[] comments)
            {
                this.prefix = prefix;
                this.comments =
                    !comments.Any()
                    ? new [] { string.Empty, }
                    : comments
                        // Split using `\r\n` CharArray rather than `Environment.NewLine` to cater
                        // for files that use a differnet standard from the current OS.
                        // Don't trim the resultant comments to allow them to have leading spaces.
                        .Select(comment => comment.Split("\r\n".ToCharArray(),
                                                         StringSplitOptions.RemoveEmptyEntries))
                        .SelectMany(comment => comment)
                        .ToArray();
            }

            /// <inheritdoc/>
            public override string[] Serialize() =>
                this.comments
                    .Select(comment => $"{prefix} {comment}")
                    .ToArray();
        }

        /// <summary>
        /// An abstract base class for serializing config data.
        /// </summary>
        private abstract class BaseConfigData : BaseSerializationData
        {
            protected Dictionary<string, string> Configuration { get; }

            protected abstract Lazy<string[]> ValidKeys { get; }

            protected BaseConfigData(Dictionary<string, string> configuration) =>
                this.Configuration = configuration;

            protected IEnumerable<KeyValuePair<string, string>> GetConfiguration() =>
                this.Configuration
                    .Where(kvp => this.ValidKeys.Value.Any(key => key == kvp.Key));

            protected static string[] GetValidKeys(Type type) =>
                type.GetFields(BindingFlags.Public |
                                BindingFlags.Static)
                    .Where(field => field.FieldType == typeof(string))
                    .Select(field => field.GetValue(null))
                    .Cast<string>()
                    .ToArray();
        }

        /// <summary>
        /// A sealed class for serializing global config data.
        /// </summary>
        private sealed class GlobalConfigData : BaseConfigData
        {
            private readonly string prefix = Converter.Keys.GlobalConfig.GlobalConfigPrefix;

            protected override Lazy<string[]> ValidKeys { get; } =
                new(() => GetValidKeys(typeof(Converter.Keys.GlobalConfig)));

            public GlobalConfigData(Dictionary<string, string> configuration)
                : base(configuration) { }

            /// <inheritdoc/>
            public override string[] Serialize() =>
                GetConfiguration()
                    .Select(kvp => $"{this.prefix},{kvp.Key},{kvp.Value}")
                    .ToArray();
        }

        /// <summary>
        /// A sealed class for serializing typed config data.
        /// </summary>
        private sealed class TypedConfigData : BaseConfigData
        {
            private readonly string prefix = Converter.Keys.TypedConfig.TypedConfigPrefix;
            private readonly string typeName;

            protected override Lazy<string[]> ValidKeys { get; } =
                new(() => GetValidKeys(typeof(Converter.Keys.TypedConfig)));

            public TypedConfigData(string typeName, Dictionary<string, string> configuration)
                : base(configuration) => this.typeName = typeName;

            /// <inheritdoc/>
            public override string[] Serialize() =>
                GetConfiguration()
                    .Select(kvp => $"{this.prefix},{this.typeName},{kvp.Key},{kvp.Value}")
                    .ToArray();
        }

        /// <summary>
        /// An abstract base class for serializing conversion data.
        /// </summary>
        private abstract class BaseConversionData : BaseSerializationData
        {
            private readonly string prefix;
            private readonly Dictionary<string, string> conversion;

            protected BaseConversionData(string prefix, Dictionary<string, string> conversion)
            {
                this.prefix = prefix;
                this.conversion = conversion;
            }

            /// <inheritdoc/>
            public override string[] Serialize() =>
                this.conversion
                    .Select(kvp => $"{this.prefix},\"{kvp.Key}\",\"{kvp.Value}\"")
                    .ToArray();
        }

        /// <summary>
        /// A sealed class for serializing type conversion data.
        /// </summary>
        private sealed class TypeConversionData : BaseConversionData
        {
            public TypeConversionData(Dictionary<string, string> typeConversion)
                : base(ConfigHandler.Configurations.ConversionTypePrefix, typeConversion) { }
        }

        /// <summary>
        /// A sealed class for serializing value conversion data.
        /// </summary>
        private sealed class ValueConversionData : BaseConversionData
        {
            public ValueConversionData(Dictionary<string, string> valueConversion)
                : base(ConfigHandler.Configurations.ConversionValuePrefix, valueConversion) { }
        }

        /// <summary>
        /// An abstract base class for serializing metadata.
        /// </summary>
        private abstract class BaseMetadataData : BaseSerializationData
        {
            /// <summary>
            /// The name of the type of the object data that this metadata is attached to.
            /// </summary>
            protected readonly string dataTypeName;

            /// <summary>
            /// The prefix to use to identify this metadata.
            /// </summary>
            protected readonly string metadataPrefix;

            /// <summary>
            /// The names of the properties of the metadata in the order that they are to be serialized.
            /// </summary>
            protected readonly string[]? propertyNames;

            protected BaseMetadataData(string dataTypeName, string metadataPrefix, string[]? propertyNames)
            {
                this.dataTypeName = dataTypeName;
                this.metadataPrefix = metadataPrefix;
                this.propertyNames = propertyNames;
            }
        }

        /// <summary>
        /// A sealed class for serializing typed metadata of <typeparamref name="TMetadata"/>.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        private sealed class TypedMetadataData<TMetadata> : BaseMetadataData
            where TMetadata : class
        {
            private readonly IEnumerable<TMetadata> metadata;
            private readonly PropertyInfo[] properties;

            public TypedMetadataData(string dataTypeName, string prefix, string[]? propertyNames, IEnumerable<TMetadata> metadata)
                : base(dataTypeName, prefix, propertyNames)
            {
                this.metadata = metadata;

                this.properties =
                    typeof(TMetadata).GetProperties(BindingFlags.Instance |
                                                    BindingFlags.Public)
                        .Where(property => property.CanRead && property.CanWrite)
                        .Where(property => this.propertyNames?.Contains(property.Name) ?? false)
                        .ToArray();
            }

            public override string[] Serialize() => 
                this.metadata
                    .Select(md => GetValues(md))
                    .Where(values => !string.IsNullOrWhiteSpace(values))
                    .Select(values => $"{this.metadataPrefix},{this.dataTypeName},{values}")
                    .ToArray();

            private string GetValues(TMetadata metadata)
            {
                if (this.propertyNames is null)
                    return string.Empty;

                // The values must be in the order defined in the Options Metadata,
                // so derive them from the Options Metadata PropertyNames,
                // rather than directly from the properties of the Type.

                var properties =
                    this.propertyNames
                        .Select(propertyName =>
                            this.properties
                                .FirstOrDefault(property => property.Name == propertyName))
                        .NotNull()
                        .ToArray();
                if (properties is null)
                    return string.Empty;

                var results =
                    ConverterHelper.AsStrings(metadata, properties)
                        .ToArray();

                var result = string.Join(",", results);

                return result;
            }
        }

        /// <summary>
        /// A sealed class for serializing typeless metadata.
        /// </summary>
        private sealed class TypelessMetadataData : BaseMetadataData
        {
            private readonly Dictionary<string, string> metadata;

            public TypelessMetadataData(string dataTypeName, string metadataPrefix, string[]? propertyNames, Dictionary<string, string> metadata)
                : base(dataTypeName, metadataPrefix, propertyNames) => this.metadata = metadata;

            /// <inheritdoc/>
            public override string[] Serialize() =>
                new[] { $"{this.metadataPrefix},{this.dataTypeName},{GetValues()}" };

            private string GetValues()
            {
                var results = this.propertyNames is null ?
                    Array.Empty<string>() :
                    this.propertyNames
                        .Select(propertyName =>
                            this.metadata.TryGetValue(propertyName, out var value)
                                ? value
                                : string.Empty)
                        .Select(value => $"\"{value}\"")
                        .ToArray();

                var result = string.Join(",", results);

                return result;
            }
        }

        /// <summary>
        /// An abstract base class for serializing data.
        /// </summary>
        private abstract class BaseDataData : BaseSerializationData
        {
            #region Fields

            protected readonly string propertyPrefix;
            protected readonly string valuePrefix;
            protected readonly string typeName;

            #endregion

            #region Abstract properties

            protected abstract string[] Names { get; }

            #endregion

            #region Constructors

            protected BaseDataData(string propertyPrefix, string valuePrefix, string typeName)
            {
                this.propertyPrefix = propertyPrefix;
                this.valuePrefix = valuePrefix;
                this.typeName = typeName;
            }

            #endregion

            #region Overrides

            /// <inheritdoc/>
            public override string[] Serialize() =>
                GetNames()
                    .Union(GetValues())
                    .ToArray();

            #endregion

            #region Abstract support routines

            protected abstract string[] GetValues();

            #endregion

            #region Support routines

            private string[] GetNames() =>
                new[] { ConverterHelper.FormatCsvData(this.propertyPrefix, this.typeName, this.Names), };

            #endregion
        }

        /// <summary>
        /// A sealed class for serializing data of <typeparamref name="TObject"/>.
        /// </summary>
        /// <typeparam name="TObject">The type of the data.</typeparam>
        private sealed class TypedDataData<TObject> : BaseDataData
            where TObject : class
        {
            #region Fields

            private readonly IEnumerable<TObject> data;
            private readonly PropertyInfo[]? properties;
            private readonly string[] names;

            #endregion

            #region Override properties

            protected override string[] Names => this.names;

            #endregion

            #region Constructors

            public TypedDataData(string propertyPrefix, string valuePrefix, string typeName, IEnumerable<TObject> data)
                : base(propertyPrefix, valuePrefix, typeName)
            {
                this.data = data;
                this.properties =
                    typeof(TObject).GetProperties(BindingFlags.Instance |
                                                  BindingFlags.Public)
                        .Where(property => property.CanRead && property.CanWrite)
                        .ToArray();
                this.names = GetNames();
            }

            #endregion

            #region Overrides

            protected override string[] GetValues() =>
                this.data
                    .Select(item => GetValues(item))
                    .Where(values => values.Any())
                    .Where(values => values.Length == this.names.Length)
                    .Select(values => ConverterHelper.FormatCsvData(this.valuePrefix, this.typeName, values))
                    .ToArray();

            #endregion

            #region Support routines

            private string[] GetNames() =>
                this.properties is null ?
                    Array.Empty<string>() :
                    this.properties
                        .Select(property => property.Name)
                        .ToArray();

            private string[] GetValues(TObject item) =>
                this.properties is null ?
                    Array.Empty<string>() :
                    ConverterHelper.AsStrings(item, this.properties)
                        .ToArray();

            #endregion
        }

        /// <summary>
        /// A sealed class for serializing typeless data.
        /// </summary>
        private sealed class TypelessDataData : BaseDataData
        {
            #region Fields

            private readonly IEnumerable<string[]> values;

            #endregion

            #region Override properties

            protected override string[] Names { get; }

            #endregion

            #region Constructors

            public TypelessDataData(string propertyPrefix, string valuePrefix, string typeName, string[] names, IEnumerable<string[]> values)
                : base(propertyPrefix, valuePrefix, typeName)
            {
                this.Names = names;
                this.values = values;
            }

            #endregion

            #region Overrides

            protected override string[] GetValues() =>
                this.values
                    .Select(values => GetValues(values))
                    .Select(values => ConverterHelper.FormatCsvData(this.valuePrefix, this.typeName, values))
                    .ToArray();

            #endregion

            #region Support routines

            private static string[] GetValues(string[] values) =>
                values
                    .Select(value => $"\"{value}\"")
                    .ToArray();

            #endregion
        }

        #endregion
    }
}
