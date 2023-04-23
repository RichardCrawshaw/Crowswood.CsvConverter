using Crowswood.CsvConverter.Exceptions;
using Crowswood.CsvConverter.Extensions;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class TypelessMetadataData : BaseMetadataData
    {
        public TypelessMetadataData(DeserializationFactory factory,
                                    string objectTypeName, string metadataTypeName)
            : base(factory, objectTypeName, metadataTypeName) { }

        /// <summary>
        /// Gets the metadata as a tuple of <see cref="string[]"/> containing the property names 
        /// and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.
        /// </summary>
        /// <returns>A tuple of <see cref="string[]"/> and <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        /// <exception cref="MetadataNotDeserializedException">If the metadata has not been deserialized.</exception>
        public (string[] Names, IEnumerable<string[]> Values) GetMetadata() =>
            this.propertyNames is not null && this.values is not null
                ? (this.propertyNames, this.values.Trim().Trim('"'))
                : throw new MetadataNotDeserializedException();

        /// <inheritdoc/>
        protected override OptionMetadata? GetOptionMetadata() =>
            this.factory.Options.GetOptionMetadata(this.MetadataTypeName);
    }
}
