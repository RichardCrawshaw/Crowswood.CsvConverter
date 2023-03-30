using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Helpers;

namespace Crowswood.CsvConverter.Processors
{
    internal class SerializationProcessor
    {
        #region Fields

        private readonly Options options;
        private readonly MetadataHandler metadataHandler;

        #endregion

        #region Constructors

        public SerializationProcessor(Options options, MetadataHandler metadataHandler)
        {
            this.options = options;
            this.metadataHandler = metadataHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes the specified <paramref name="values"/> to a <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The base type of objects to serialize.</typeparam>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> to serialize.</param>
        /// <returns>A <see cref="string"/>.</returns>
        internal string Process<TBase>(params TBase[] values) where TBase : class
        {
            var lines = new List<string>();

            lines.AddRange(this.options.OptionSerialize.ToArray());

            var types = this.options.OptionTypes.Any(ot => ot.Type != typeof(Type))
                ? this.options.OptionTypes
                    .Where(ot => ot.Type != typeof(Type))
                    .Select(optionType => optionType.Type)
                : values
                    .Select(value => value.GetType())
                    .Distinct();

            foreach (var type in types)
            {
                lines.AddRange(ConvertFrom(type, values));
                lines.Add(string.Empty);
            }

            var text = string.Join(Environment.NewLine, lines);
            return text;
        }

        /// <summary>
        /// Serializes the specified <paramref name="data"/> to a <see cref="string"/>.
        /// </summary>
        /// <param name="data">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="Tuple{T1, T2}"/> of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> keyed by <see cref="string"/>.</param>
        /// <returns>A <see cref="string"/>.</returns>
        internal string Process(Dictionary<string, (string[], IEnumerable<string[]>)> data)
        {
            var lines =
                this.options.OptionSerialize
                    .ToArray()
                    .ToList();
            lines.AddRange(
                data.Select(kvp => Process(kvp.Key, kvp.Value.Item1, kvp.Value.Item2))
                    .SelectMany(item => item));

            var text = string.Join(Environment.NewLine, lines);
            return text;
        }

        #endregion

        #region Conversion routines

        /// <summary>
        /// Converts the specified <paramref name="values"/> of type <paramref name="type"/> into
        /// an <see cref="IEnumerable{T}"/> of <see cref="string"/>.
        /// </summary>
        /// <typeparam name="TBase">The type of object to process.</typeparam>
        /// <param name="type">The <see cref="Type"/> of object; it must be assignable to <typeparamref name="TBase"/>.</param>
        /// <param name="values">An <see cref="IEnumerable{T}"/> of <typeparamref name="TBase"/> that contains the objects to process.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        /// <exception cref="ArgumentException">If <seealso cref="OptionType.Type"/> cannot be assigned to <typeparamref name="TBase"/>.</exception>
        /// <exception cref="InvalidOperationException">If the conversion failed.</exception>
        /// <remarks>This routine calls its namesake by reflection.</remarks>
        private IEnumerable<string> ConvertFrom<TBase>(Type type, IEnumerable<TBase> values)
            where TBase : class //  can't use a new() contraint as TBase may well be abstract
        {
            if (!type.IsAssignableTo(typeof(TBase)))
                throw new ArgumentException(
                    $"Unable to assign an object of type {type.Name} to {typeof(TBase).Name}.");

            var properties = type.GetReadWritePropertiesByDepth();
            var parameters = properties.GetParameters(this.options);
            var typeName = GetTypeName(type);

            var results =
                values
                    .NotNull()
                    .Where(value => value.GetType().Equals(type))
                    .Select(value => value.AsStrings(properties))
                    .Select(items => ConverterHelper.AsStrings(items).ToArray())
                    .Select(value => ConverterHelper.FormatCsvData(this.options.ValuesPrefix, typeName, value))
                    .ToList();
            results.Insert(0,
                ConverterHelper.FormatCsvData(this.options.PropertyPrefix, typeName, parameters));

            return results;
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Gets the type name for the specified <paramref name="type"/> with any conversion that 
        /// is defined in the <seealso cref="options"/> or as an attribute on the type itself.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to get the name of.</param>
        /// <returns>A <see cref="string"/> containing the type name.</returns>
        private string GetTypeName(Type type) =>
            this.options.GetOptionType(type)?.Name ?? 
            type.GetTypeName();

        /// <summary>
        /// Serializes the specified <paramref name="typeName"/> with the specified <paramref name="propertyNames"/> 
        /// and <paramref name="dataValues"/> including any metadata.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the name of the data type.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the names of the properties.</param>
        /// <param name="dataValues">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the data values.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="string"/>.</returns>
        private List<string> Process(string typeName, string[] propertyNames, IEnumerable<string[]> dataValues)
        {
            List<string> results = new();

            // If there is any metadata for this type then add that first.
            if (this.metadataHandler.Metadata.TryGetValue(typeName, out var metadataItems))
            {
                var metadataValues =
                    metadataItems
                        .Select(item => new
                        {
                            Metadata = this.options.GetOptionMetadata(item.GetType()),
                            Item = item,
                        })
                        .Where(n => n.Metadata?.Prefix is not null)
                        .Select(n => new
                        {
                            n.Metadata!.Prefix,
                            Values = n.Item.GetValues(n.Metadata.PropertyNames),
                        })
                        .Select(n => ConverterHelper.FormatCsvData(n.Prefix, typeName, n.Values));
                results.AddRange(metadataValues);
            }

            // Add the properties line.
            results.Add(
                ConverterHelper.FormatCsvData(this.options.PropertyPrefix,
                                              typeName,
                                              propertyNames));

            // Finally add all the values lines.
            results.AddRange(
                dataValues
                    .Select(items => items.Select(item => $"\"{item}\"").ToArray())
                    .Select(values => ConverterHelper.FormatCsvData(this.options.ValuesPrefix,
                                                                    typeName,
                                                                    values)));

            // Add a blank line to separate the block of data for this type from any others.
            results.Add(string.Empty);

            return results;
        }

        #endregion
    }
}
