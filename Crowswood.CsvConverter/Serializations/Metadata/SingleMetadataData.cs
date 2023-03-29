namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstract class for serializing a single data type of metadata.
    /// </summary>
    internal abstract class SingleMetadataData : BaseMetadataData
    {
        /// <summary>
        /// The name of the type of the metadata.
        /// </summary>
        internal abstract string MetadataTypeName { get; }

        protected SingleMetadataData(Serialization.SerializationFactory prefixFactory, string dataTypeName)
            : base(prefixFactory, dataTypeName) { }
    }
}
