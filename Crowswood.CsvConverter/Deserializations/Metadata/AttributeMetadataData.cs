using Crowswood.CsvConverter.Extensions;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class AttributeMetadataData : BaseMetadataData
    {
        public Type MetadataType { get; }

        public AttributeMetadataData(DeserializationFactory factory, string objectTypeName, Type metadataType)
            : base(factory, objectTypeName, metadataType.GetTypeName()) => this.MetadataType = metadataType;

        /// <inheritdoc/>
        protected override OptionMetadata? GetOptionMetadata() =>
            this.factory.Options.GetOptionMetadata(this.MetadataType);
    }
}
