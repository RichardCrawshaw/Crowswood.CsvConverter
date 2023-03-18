using System.Reflection;
using System.Runtime.CompilerServices;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Interfaces;
using Crowswood.CsvConverter.Processors;
using static Crowswood.CsvConverter.Handlers.ConfigHandler;

[assembly: 
    InternalsVisibleTo("Crowswood.CsvConverter.Tests")]

namespace Crowswood.CsvConverter
{
    /// <summary>
    /// Converter class for CSV data that supports comments and multiple data-types in a single
    /// data-stream. Comments and blank lines are ignored. Each data-type requires a properties
    /// line and multiple values lines; each value line represents a single object instance. 
    /// Property and values lines are distinguished by different prfixes: these can be configured, 
    /// but have usable defaults. Each data-type is distinguished by the name of the data-type; 
    /// this follows the property / values prefix. Example:
    /// Properties,Foo,Field1,Field2,...
    /// Values,Foo,Value1,Value2,...
    /// </summary>
    /// <remarks>
    /// NOTE: Comments are NOT written when the data is serialized, nor are blank lines retained; 
    /// this is because the comments and blank lines are not tracked. Also, the data may have 
    /// changed and it is not possible to know whether they are still relevent to the new data.
    /// </remarks>
    public sealed class Converter
    {
        #region Fields

        private readonly Options options;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the metadata as a dictionary keyed by <see cref="string"/> which contains the 
        /// type name.
        /// </summary>
        public Dictionary<string, List<object>> Metadata { get; } = new();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="Converter"/> according to the specified
        /// <paramref name="options"/>.
        /// </summary>
        /// <param name="options">An <see cref="options"/> object.</param>
        public Converter(Options options)
        {
            this.options = OptionsHelper.ValidateOptions(options);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines and returns whether the specified <paramref name="text"/> appears to be 
        /// valid CSV.
        /// </summary>
        /// <param name="text">A <see cref="string"/> that contains the text to check.</param>
        /// <returns>True if the text appears to be valid CSV; false otherwise.</returns>
        public bool Check(string text)
        {
            ValidateText(text);
            var trimmedText = text.Trim();

            // Specifically exclude text that looks like either JSON or XML.
            if (trimmedText.StartsWith('{') && trimmedText.EndsWith('}')) return false;
            if (trimmedText.StartsWith('[') && trimmedText.EndsWith(']')) return false;
            if (trimmedText.StartsWith('<') && trimmedText.EndsWith('>')) return false;

            var lines = SplitLines(text);

            // Are there at least two lines, and do they all, once comments and blank lines have
            // been removed, contain a comma?
            return 
                lines.Count >= 2 &&
                lines.All(line => line.Contains(','));
        }

        /// <summary>
        /// Deserialize the specified <paramref name="text"/> into an <see cref="IEnumerable{T}"/> 
        /// of <typeparamref name="TBase"/> objects.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to return.</typeparam>
        /// <param name="text">A <see cref="string"/> that contains the data to deserialize.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/>.</returns>
        public IEnumerable<TBase> Deserialize<TBase>(string text)
            where TBase : class
        {
            this.Metadata.Clear();

            ValidateText(text);
            var lines = SplitLines(text);

            var configHandler = new ConfigHandler(this.options, lines);
            var conversionHandler = new ConversionHandler(this.options, configHandler, lines);
            var indexHandler = new IndexHandler();
            var metadataHandler = new MetadataHandler(this.options, conversionHandler, indexHandler);

            var types =
                this.options.OptionTypes.Any()
                ? this.options.OptionTypes.Select(optionType => optionType.Type)
                : GetTypes<TBase>(configHandler, lines);

            if (types.Any(type => type == typeof(Type)))
                throw new InvalidOperationException("Dynamic types included.");

            var processor =
                new DeserializationProcessor(this.options,
                                             configHandler,
                                             conversionHandler,
                                             indexHandler,
                                             metadataHandler,
                                             lines);
            var results = processor.Process<TBase>(types);

            foreach (var item in metadataHandler.Metadata)
                this.Metadata.Add(item.Key, item.Value);

            return results;
        }

        /// <summary>
        /// Deserialize the specified <paramref name="text"/> into a <see cref="Dictionary{TKey, TValue}"/> 
        /// of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> 
        /// of <see cref="string[]"/>, keyed by <see cref="string"/>.
        /// </summary>
        /// <param name="text">A <see cref="string"/> that contains the data to deserialize.</param>
        /// <returns>A  <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/>, keyed by <see cref="string"/>.</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="text"/> is empty.
        /// or
        /// If there are no types defined in the options, or if non-dynamic types are included.</exception>
        public Dictionary<string, (string[], IEnumerable<string[]>)> Deserialize(string text)
        {
            this.Metadata.Clear();

            ValidateText(text);
            var lines = SplitLines(text);

            var configHandler = new ConfigHandler(this.options, lines);
            var conversionHandler = new ConversionHandler(this.options, configHandler, lines);
            var indexHandler = new IndexHandler();
            var metadataHandler = new MetadataHandler(this.options, conversionHandler, indexHandler);

            var processor =
                new DeserializationProcessor(this.options,
                                             configHandler,
                                             conversionHandler,
                                             indexHandler,
                                             metadataHandler,
                                             lines);
            var results = processor.Process();

            foreach (var item in metadataHandler.Metadata)
                this.Metadata.Add(item.Key, item.Value);

            return results;
        }

        /// <summary>
        /// Serializes the specified <paramref name="values"/> to a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to serialize.</typeparam>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> to serialize.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public string Serialize<TBase>(IEnumerable<TBase> values) where TBase : class
        {
            var configHandler = new ConfigHandler(this.options, new List<string>());
            var conversionHandler = new ConversionHandler(this.options, configHandler, new List<string>());
            var indexHandler = new IndexHandler();
            var metadataHandler = new MetadataHandler(this.options, conversionHandler, indexHandler);

            var processor = new SerializationProcessor(this.options, metadataHandler);
            var result = processor.Process(values.ToArray());
            return result;
        }

        /// <summary>
        /// Serializes the specified <paramref name="data"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> keyed by <see cref="string"/>.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public string Serialize(Dictionary<string, (string[], IEnumerable<string[]>)> data)
        {
            var configHandler = new ConfigHandler(this.options, new List<string>());
            var conversionHandler = new ConversionHandler(this.options, configHandler, new List<string>());
            var indexHandler = new IndexHandler();
            var metadataHandler = new MetadataHandler(this.options, conversionHandler, indexHandler);

            var processor = new SerializationProcessor(this.options, metadataHandler);
            var result = processor.Process(data);
            return result;
        }

        /// <summary>
        /// Initialises and retrieves an <see cref="ISerialization"/> object that can be used to 
        /// manage the serialization of multiple objects, including configuration, conversions, 
        /// typed and typeless metadata, typed and typeless data, and comments.
        /// </summary>
        /// <returns>An <see cref="ISerialization"/> object.</returns>
        public ISerialization Serialize() => new Serialization(this, this.options);

        #endregion

        #region Nested classes

        /// <summary>
        /// Class that defines the keys used in the data dictionaries.
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// Keys used in the global config data dictionary.
            /// </summary>
            public static class GlobalConfig
            {
                public const string ConversionTypePrefix = Configurations.ConversionTypePrefix;
                public const string ConversionValuePrefix = Configurations.ConversionValuePrefix;
                public const string PropertyPrefix = Configurations.PropertyPrefix;
                public const string ReferenceIdColumnName = Configurations.ReferenceIdColumnName;
                public const string ReferenceNameColumnName = Configurations.ReferenceNameColumnName;
                public const string ValuesPrefix = Configurations.ValuesPrefix;

                internal const string GlobalConfigPrefix = Configurations.GlobalConfigPrefix;
            }

            /// <summary>
            /// Keys used in the typed config data dictionary.
            /// </summary>
            public static class TypedConfig
            {
                public const string ConversionTypePrefix = Configurations.ConversionTypePrefix;
                public const string ConversionValuePrefix = Configurations.ConversionValuePrefix;
                public const string PropertyPrefix = Configurations.PropertyPrefix;
                public const string ReferenceIdColumnName = Configurations.ReferenceIdColumnName;
                public const string ReferenceNameColumnName = Configurations.ReferenceNameColumnName;
                public const string ValuesPrefix = Configurations.ValuesPrefix;

                internal const string TypedConfigPrefix = Configurations.TypedConfigPrefix;
            }
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Gets the types that exist in the <see cref="Assembly"/> that contains <typeparamref name="TBase"/>
        /// that exist in the specified <paramref name="lines"/>.
        /// </summary>
        /// <typeparam name="TBase">The base type.</typeparam>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/>.</returns>
        private static IEnumerable<Type> GetTypes<TBase>(ConfigHandler configHandler, IEnumerable<string> lines)
            where TBase : class
        {
            var typeNames =
                ConverterHelper.GetTypeNames(lines,
                                              configHandler.GetPropertyPrefixes(),
                                              tn => configHandler.GetPropertyPrefix(tn));
            var types =
                Assembly.GetAssembly(typeof(TBase))?.GetTypes()
                    .Select(type => new
                    {
                        Type = type,
                        Attribute = type.GetCustomAttribute<CsvConverterClassAttribute>(),
                    })
                    .Where(n => typeNames.Contains(n.Type.Name) ||
                                typeNames.Contains(n.Attribute?.Name))
                    .Select(n => n.Type) ?? new List<Type>();
            return types;
        }

        /// <summary>
        /// Splits the specified <paramref name="text"/> into lines on end of line characters, 
        /// removing any empty entries, trimming them and ignoring any that are comments.
        /// </summary>
        /// <param name="text">A <see cref="string"/> containing the text to split.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        private List<string> SplitLines(string text) =>
            // Split using `\r\n` CharArray rather than `Environment.NewLine` to cater for files
            // that use a differnet standard from the current OS.
            text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries |
                                             StringSplitOptions.TrimEntries)
                .Where(line => !this.options.CommentPrefixes.Any(prefix => line.StartsWith(prefix)))
                .ToList();

        /// <summary>
        /// Validates that the specified <paramref name="text"/> is not empty.
        /// </summary>
        /// <param name="text">A <see cref="string"/> that contains the text to validate.</param>
        /// <exception cref="InvalidOperationException">If the <paramref name="text"/> is not valid.</exception>
        private static void ValidateText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Text is empty.");
        }

        #endregion
    }
}
