namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstract base class for serializing metadata.
    /// </summary>
    internal abstract class BaseMetadataData : BaseSerializationData
    {
        protected readonly Serialization.SerializationFactory prefixFactory;

        /// <summary>
        /// The name of the type of the object data that this metadata is attached to.
        /// </summary>
        protected readonly string dataTypeName;

        /// <summary>
        /// The name of the type of the metadata.
        /// </summary>
        internal abstract string MetadataTypeName { get; }

        /// <summary>
        /// Gets the name of the object data type that this metadata is attached to.
        /// </summary>
        internal string ObjectDataTypeName => this.dataTypeName;

        protected BaseMetadataData(Serialization.SerializationFactory prefixFactory, string dataTypeName)
        {
            this.prefixFactory = prefixFactory;
            this.dataTypeName = dataTypeName;
        }
    }
}
