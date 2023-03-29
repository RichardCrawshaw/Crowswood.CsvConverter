using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing typeless metadata.
    /// </summary>
    internal sealed class TypelessMetadataData : SingleMetadataData
    {
        private readonly string metadataPrefix;
        private readonly Dictionary<string, string> metadata;

        internal override string MetadataTypeName => this.metadataPrefix;

        public TypelessMetadataData(Serialization.SerializationFactory prefixFactory, string dataTypeName, string metadataPrefix, Dictionary<string, string> metadata)
            : base(prefixFactory, dataTypeName)
        {
            this.metadataPrefix = metadataPrefix;
            this.metadata = metadata;
        }

        /// <inheritdoc/>
        public override string[] Serialize()
        {
            var optionMetadata =
                this.factory.Options.GetOptionMetadata(metadataPrefix) ??
                throw new InvalidOperationException(
                    $"No Metadata definition in Options for a prefix of '{metadataPrefix}'.");

            var result = 
                GetValues(this.metadata, optionMetadata.PropertyNames)
                    .AsCsv(optionMetadata.Prefix, this.dataTypeName);

            return new[] { result, };
        }

        /// <summary>
        /// Gets the property values from the specified <paramref name="metadata"/> dictionary 
        /// using the specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="metadata">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> containing the metadata.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the property names.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private static string[] GetValues(Dictionary<string, string> metadata, string[] propertyNames) =>
            propertyNames
                .Select(propertyName => Getvalue(metadata, propertyName))
                .Select(value => $"\"{value}\"")
                .ToArray();

        /// <summary>
        /// Gets a value from the specified <paramref name="propertyName"/> using the specified 
        /// <paramref name="propertyName"/> or empty string if the metadata does not contain an 
        /// item with the property name.
        /// </summary>
        /// <param name="metadata">A <see cref="Dictionary{TKey, TValue}"/> of <see cref="string"/> keyed by <see cref="string"/> containing the metadata.</param>
        /// <param name="propertyName">A <see cref="string"/> containing the property name.</param>
        /// <returns>A <see cref="string"/>.</returns>
        private static string Getvalue(Dictionary<string, string> metadata, string propertyName) =>
            metadata.TryGetValue(propertyName, out var value) ? value : string.Empty;
    }
}
