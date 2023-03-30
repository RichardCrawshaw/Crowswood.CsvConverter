using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstact class that provides the base class for the generic typed metadata variants.
    /// </summary>
    internal abstract class TypedMetadataData : SingleMetadataData
    {
        /// <summary>
        /// Gets the type of metadata that the current instance serializes.
        /// </summary>
        public abstract Type MetadataType { get; }

        protected TypedMetadataData(Serialization.SerializationFactory prefixFactory, string dataTypeName)
            : base(prefixFactory, dataTypeName) { }
    }

    /// <summary>
    /// A sealed class for serializing typed metadata of <typeparamref name="TMetadata"/>.
    /// </summary>
    /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
    internal sealed class TypedMetadataData<TMetadata> : TypedMetadataData
        where TMetadata : class
    {
        private readonly IEnumerable<TMetadata> metadata;

        internal override string MetadataTypeName => typeof(TMetadata).Name;

        public override Type MetadataType => typeof(TMetadata);

        public TypedMetadataData(Serialization.SerializationFactory factory, string dataTypeName, IEnumerable<TMetadata> metadata)
            : base(factory, dataTypeName) => this.metadata = metadata;

        /// <inheritdoc/>
        public override string[] Serialize()
        {
            var optionMetadata =
                this.factory.Options.GetOptionMetadata(this.MetadataType) ??
                throw new InvalidOperationException(
                    $"No Metadata definition in Options for '{this.MetadataTypeName}'.");

            var properties = typeof(TMetadata).GetPropertiesByName(optionMetadata.PropertyNames);

            var metadata =
                this.metadata
                    .Select(value => value.AsStrings(properties));

            var result = Serialize(metadata, optionMetadata.Prefix);
            //var result = Serialize(this.metadata, optionMetadata.Prefix, properties);
            return result;
        }
    }
}
