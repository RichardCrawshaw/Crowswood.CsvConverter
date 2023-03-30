using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Helpers;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstract base class for serializing metadata.
    /// </summary>
    internal abstract class BaseMetadataData : BaseSerializationData
    {
        protected readonly Serialization.SerializationFactory factory;

        /// <summary>
        /// Gets the name of the object data type that this metadata is attached to.
        /// </summary>
        internal string ObjectDataTypeName { get; }

        protected BaseMetadataData(Serialization.SerializationFactory factory, string dataTypeName)
        {
            this.factory = factory;
            this.ObjectDataTypeName = dataTypeName;
        }

        /// <summary>
        /// Serializes the specified <paramref name="metadata"/> using the specified <paramref name="metadataPrefix"/>.
        /// </summary>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <see cref="IEnumerable{T}"/> of <see cref="Type"/> and <see cref="object"/> that contains the metadata.</param>
        /// <param name="metadataPrefix">A <see cref="string"/> containing the metadata prefix.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        protected string[] Serialize(IEnumerable<IEnumerable<(Type Type, object? Value)>> metadata, string metadataPrefix) =>
            metadata
                .Select(items => ConverterHelper.AsStrings(items))
                .Select(values => values.ToArray())
                .Where(values => values.Any())
                .Select(values => values.AsCsv(metadataPrefix, this.ObjectDataTypeName))
                .ToArray();
    }
}
