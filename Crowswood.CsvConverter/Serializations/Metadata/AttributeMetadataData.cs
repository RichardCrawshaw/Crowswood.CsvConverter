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
        public override string[] Serialize()
        {
            var metadata =
                this.objectType.GetAttributes();
            var types =
                metadata
                    .Select(md => md.GetType())
                    .Distinct();

            List<string> results = new();
            foreach(var type in types)
            {
                var optionsMetadata = this.factory.Options.GetOptionMetadata(type);
                if (optionsMetadata is null)
                    continue;

                var properties = type.GetReadWriteProperties(optionsMetadata.PropertyNames);

                var values =
                    metadata
                        .Where(md => md.GetType() == type)
                        .Select(md => md.AsStrings(properties));

                var serializedMetadata = Serialize(values, optionsMetadata.Prefix);
                results.AddRange(serializedMetadata);
            }
            return results.ToArray();
        }
    }
}
