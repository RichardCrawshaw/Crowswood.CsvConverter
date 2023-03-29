using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing metadata that is connected to its data object by virtue of 
    /// being an <see cref="Attribute"/> attached to the data object <see cref="Type"/>.
    /// </summary>
    internal sealed class AttributeMetadataData : BaseMetadataData
    {
        private readonly Type objectType;

        public AttributeMetadataData(Serialization.SerializationFactory factory, string dataTypeName, Type objectType)
            : base(factory, dataTypeName) => this.objectType = objectType;

        /// <inheritdoc/>
        public override string[] Serialize() => 
            this.objectType.GetAttributes()
                .Select(metadata => new
                {
                    Metadata = metadata,
                    Type = metadata.GetType()
                })
                .GroupBy(item => item.Type)
                .Select(group => new 
                { 
                    Metadata = group.Select(item => item.Metadata), 
                    OptionsMetadata = this.factory.Options.GetOptionMetadata(group.Key),
                    Type = group.Key,
                })
                .Where(item => item.OptionsMetadata is not null)
                .Select(item => Serialize(item.Metadata, item.OptionsMetadata!, item.Type))
                .SelectMany(item => item)
                .ToArray();

        /// <summary>
        /// Serializes the specified <paramref name="metadata"/> using the specified <paramref name="optionMetadata"/> 
        /// and <paramref name="type"/>.
        /// </summary>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <see cref="Attribute"/>.</param>
        /// <param name="optionMetadata">An <see cref="OptionMetadata"/> instance.</param>
        /// <param name="type">The <see cref="Type"/> of the <paramref name="metadata"/>.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] Serialize(IEnumerable<Attribute> metadata, OptionMetadata optionMetadata, Type type) =>
            Serialize(metadata,
                      optionMetadata.Prefix,
                      GetProperties(type, optionMetadata.PropertyNames));
    }
}
