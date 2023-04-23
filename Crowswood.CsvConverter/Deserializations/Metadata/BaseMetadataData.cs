using Crowswood.CsvConverter.Exceptions;
using Crowswood.CsvConverter.Extensions;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal abstract class BaseMetadataData : BaseDeserializationData
    {
        #region Fields

        protected string[]? propertyNames;

        protected IEnumerable<string[]>? values;

        #endregion

        #region Properties

        /// <summary>
        /// The name of the object data type that this current instance is attached to.
        /// </summary>
        public string ObjectTypeName { get; }

        /// <summary>
        /// Gets number of records.
        /// </summary>
        public int Count => this.values?.Count() ?? throw new MetadataNotDeserializedException();

        /// <summary>
        /// Gets the name of the data type.
        /// </summary>
        public string MetadataTypeName { get; }

        #endregion

        #region Constructors

        protected BaseMetadataData(DeserializationFactory factory, string objectTypeName, string metadataTypeName)
            : base(factory)
        {
            this.ObjectTypeName = objectTypeName;
            this.MetadataTypeName = metadataTypeName;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Deserialize()
        {
            var optionMetadata = GetOptionMetadata();
            if (optionMetadata is null)
            {
                this.propertyNames = Array.Empty<string>();
                this.values = Enumerable.Empty<string[]>();
                return;
            }

            // The property names are defined in the Options, not the CSV data.
            this.propertyNames = optionMetadata.PropertyNames;

            var items = GetItems(this.ObjectTypeName, optionMetadata.Prefix);

            this.values = items.GetValues(optionMetadata.Prefix, this.ObjectTypeName);
        }

        #endregion

        #region Support routines

        /// <summary>
        /// Gets the items relating to the current metadata.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        protected IEnumerable<string[]> GetItems() => GetItems(this.ObjectTypeName, GetPrefixes());

        /// <summary>
        /// Gets the <see cref="OptionMetadata"/> for this metadata type.
        /// </summary>
        /// <returns>An <see cref="OptionMetadata"/> object; null if one is not found.</returns>
        protected abstract OptionMetadata? GetOptionMetadata();

        /// <summary>
        /// Gets the prefixes that relate to metadata.
        /// </summary>
        /// <returns>A <see cref="string[]"/>.</returns>
        protected virtual string[] GetPrefixes() =>
            this.factory.Options.OptionMetadata
                .Select(om => om.Prefix)
                .ToArray();

        #endregion
    }
}
