using System.Text.RegularExpressions;
using Crowswood.CsvConverter.Exceptions;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Helpers;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal abstract class BaseObjectData : BaseDeserializationData
    {
        #region Fields

        private static readonly Regex _refDataTypeNameRegex = // #ref-data-type(lookup-value)
            new(@"^#(\w+)\((\w+)\)$", RegexOptions.Compiled);

        protected string[]? propertyNames;
        protected IEnumerable<string[]>? values;

        protected readonly Lazy<IEnumerable<string[]>> lazyValues;

        protected readonly List<BaseMetadataData> metadata = new();

        private int autoIndex = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the metadata associated with the current instance.
        /// </summary>
        public BaseMetadataData[] Metadata => this.metadata.ToArray();

        /// <summary>
        /// Gets the name of the type of the current object instance.
        /// </summary>
        public abstract string ObjectTypeName { get; }

        /// <summary>
        /// Gets the property names of the current instance.
        /// </summary>
        public string[] PropertyNames =>
            propertyNames ??
            throw new DataMustBeDeserializedException();

        #endregion

        #region Constructors

        protected BaseObjectData(DeserializationFactory factory)
            : base(factory) => this.lazyValues = new(() => GetValues());

        protected BaseObjectData(BaseObjectData source)
            : base(source.factory)
        {
            this.propertyNames = source.PropertyNames;
            this.values = source.values;
            this.lazyValues = source.lazyValues;
            this.metadata = source.metadata;
            this.autoIndex = source.autoIndex;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public sealed override void Deserialize()
        {
            var propertiesPrefix = GetPropertiesPrefix();
            var valuesPrefix = GetValuesPrefix();

            // Get the items that relate to this data type.
            var items = GetItems(this.ObjectTypeName, propertiesPrefix, valuesPrefix);

            this.propertyNames = GetPropertyNames(items, propertiesPrefix);

            this.values = GetValues(items, valuesPrefix);

            DeserializeMetadata();
        }

        #endregion

        #region Protected support routines

        /// <summary>
        /// Gets the property names from the specified <paramref name="items"/> identified by the 
        /// specified <paramref name="propertiesPrefix"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containging the items.</param>
        /// <param name="propertiesPrefix">A <see cref="string"/> containing the prefix that identifies the properties.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        protected string[] GetPropertyNames(IEnumerable<string[]> items, string propertiesPrefix) =>
            items.GetPropertyNames(propertiesPrefix, this.ObjectTypeName);

        /// <summary>
        /// Get the properties prefix to use to identify the properties relating to this data type.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the properties prefix.</returns>
        protected string GetPropertiesPrefix() =>
            ConfigHelper.GetPropertyPrefix(this.factory.GlobalConfig,
                                           this.factory.TypedConfig,
                                           this.factory.Options,
                                           this.ObjectTypeName);

        /// <summary>
        /// Gets the values from the specified <paramref name="items"/> identified by the 
        /// specified <paramref name="valuesPrefix"/> including auto-indexing and value conversion.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containging the items.</param>
        /// <param name="valuesPrefix">A <see cref="string"/> containing the prefix that identifies the values.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        /// <remarks>
        /// Reference data lookup is deferred until all data has been deserialized.
        /// </remarks>
        protected IEnumerable<string[]> GetValues(IEnumerable<string[]> items, string valuesPrefix) =>
            items
                .GetValues(valuesPrefix, this.ObjectTypeName)
                .Select(values => AutoIndex(values))
                .Select(values => ConvertValues(values));

        /// <summary>
        /// Gets the values prefix to use to identify the values relating to this data type.
        /// </summary>
        /// <returns></returns>
        protected string GetValuesPrefix() =>
            ConfigHelper.GetValuePrefix(this.factory.GlobalConfig,
                                        this.factory.TypedConfig,
                                        this.factory.Options,
                                        this.ObjectTypeName);

        /// <summary>
        /// Process the metadata by populating <seealso cref="metadata"/> and then calling 
        /// <seealso cref="BaseMetadataData.Deserialize"/> on each.
        /// </summary>
        /// <remarks>
        /// We need to populate <seealso cref="metadata"/> as we need to retrieve the data from 
        /// its elements later on.
        /// </remarks>
        protected virtual void PrepareMetadata()
        {
            this.metadata.Clear();
            this.metadata.AddRange(
                this.factory.Options.OptionMetadata
                    .Where(om => om.GetType().IsGenericType)
                    .Select(om => new TypedMetadataData(this.factory.Clone(), this.ObjectTypeName, om.Type)));
            this.metadata.AddRange(
                this.factory.Options.OptionMetadata
                    .Select(om => om as OptionTypelessMetadata)
                    .NotNull()
                    .Select(otm => new TypelessMetadataData(this.factory.Clone(), this.ObjectTypeName, otm.Prefix)));
        }

        /// <summary>
        /// Performs reference lookup on the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="values">A <see cref="string[]"/> containing the values to check for references to other data types.</param>
        /// <returns>A <see cref="string[]"/> containing the updated <paramref name="values"/>.</returns>
        protected string[] ReferenceLookup(string[] values) =>
            values
                .Select(value =>
                {
                    var matches = _refDataTypeNameRegex.Matches(value);
                    if (matches.Count == 0) return value;

                    var referenceDataTypeName = matches[0].Value;
                    var referenceValue = matches[1].Value;
                    var referenceObjectData = this.factory.GetObjectData(referenceDataTypeName);

                    var foreignKeyValue =
                        referenceObjectData?.Lookup(referenceValue,
                                                    referenceDataTypeName) ??
                        value;

                    return foreignKeyValue;
                })
                .ToArray();

        #endregion

        #region Private support routines

        /// <summary>
        /// Applies an incrementing index to the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="values">A <see cref="string[]"/> containing the values.</param>
        /// <returns>A <see cref="string[]"/> containing the updated values.</returns>
        /// <remarks>
        /// Any individual element that is just `#` is replaced with the index value.
        /// The index value is incremented, but only if  one or more of the values elements would 
        /// cause it to be used.
        /// </remarks>
        private string[] AutoIndex(string[] values)
        {
            var autoIndex =
                values.Any(value => value == "#")
                ? (this.autoIndex++).ToString()
                : string.Empty;

            var result =
                values
                    .Select(value => value == "#" ? autoIndex : value)
                    .ToArray();
            return result;
        }

        /// <summary>
        /// Performs value conversion on the specified <paramref name="values"/>.
        /// </summary>
        /// <param name="values">A <see cref="string[]"/> containing the values.</param>
        /// <returns>A <see cref="string[]"/> containing the updated values.</returns>
        private string[] ConvertValues(string[] values) =>
            values
                .Select(value =>
                    ConversionHelper.ConvertValue(value,
                                                  this.factory.Options.IsValueConversionEnabled,
                                                  this.factory.ConversionValues))
                .ToArray();

        /// <summary>
        /// Gets the values after performing reference lookup on them from the <seealso cref="values"/>.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="[]"/>.</returns>
        /// <exception cref="InvalidOperationException">If the data has not yet been deserialized.</exception>
        private IEnumerable<string[]> GetValues() =>
            this.values?
                .Select(values => ReferenceLookup(values)) ??
            throw new DataMustBeDeserializedException();

        /// <summary>
        /// Perform foreign key lookup of the specified <paramref name="referenceValue"/> on the 
        /// object data identified by the specified <paramref name="referenceDataTypeName"/>.
        /// </summary>
        /// <param name="referenceValue">A <see cref="string"/> containing the value to look up.</param>
        /// <param name="referenceDataTypeName">A <see cref="string"/> containing the name of the reference data type.</param>
        /// <returns>A nullable <see cref="string"/> containing the result of the look up, or null if there is no corresponding value.</returns>
        private string? Lookup(string referenceValue, string referenceDataTypeName)
        {
            var referenceNameColumnName =
                ConfigHelper.GetReferenceNameColumnName(this.factory.GlobalConfig,
                                                        this.factory.TypedConfig,
                                                        this.factory.Options,
                                                        referenceDataTypeName);
            var referenceIdColumnName =
                ConfigHelper.GetReferenceIdColumnName(this.factory.GlobalConfig,
                                                      this.factory.TypedConfig,
                                                      this.factory.Options,
                                                      referenceDataTypeName);

            if (referenceNameColumnName is null ||
                referenceIdColumnName is null ||
                this.propertyNames is null ||
                this.values is null) return null;

            var propertyNames =
                this.propertyNames
                    .ToList();

            var referenceNameColumnIndex = propertyNames.IndexOf(referenceNameColumnName);
            var referenceIdColumnIndex = propertyNames.IndexOf(referenceIdColumnName);

            if (referenceNameColumnIndex == -1 ||
                referenceIdColumnIndex == -1) return null;

            var value =
                this.values
                    .Where(value => value[referenceNameColumnIndex] == referenceValue)
                    .Select(value => value[referenceIdColumnIndex])
                    .FirstOrDefault();

            return value;
        }

        /// <summary>
        /// Deserialize the metadata.
        /// </summary>
        private void DeserializeMetadata()
        {
            PrepareMetadata();

            foreach (var metadata in this.metadata)
                metadata.Deserialize();

            this.metadata
                .RemoveAll(md => md.Count == 0);
        }

        #endregion
    }
}
