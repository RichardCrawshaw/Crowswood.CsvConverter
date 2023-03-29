using System.Reflection;
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
        /// The name of the type of the object data that this metadata is attached to.
        /// </summary>
        protected readonly string dataTypeName;

        /// <summary>
        /// Gets the name of the object data type that this metadata is attached to.
        /// </summary>
        internal string ObjectDataTypeName => this.dataTypeName;

        protected BaseMetadataData(Serialization.SerializationFactory factory, string dataTypeName)
        {
            this.factory = factory;
            this.dataTypeName = dataTypeName;
        }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> from the specified <paramref name="type"/> that 
        /// exist in the specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="type">The <see cref="MetadataType"/> to get the properties of.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the property names.</param>
        /// <returns>A <see cref="PropertyInfo[]"/>.</returns>
        protected static PropertyInfo[] GetProperties(Type type, string[] propertyNames) =>
            type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.CanRead && property.CanWrite)
                .Where(property => propertyNames?.Contains(property.Name) ?? false)
                .ToArray();

        /// <summary>
        /// Serializes the specified <paramref name="metadata"/> using the specified <paramref name="metadataPrefix"/> 
        /// and <paramref name="properties"/>.
        /// </summary>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <see cref="object"/>.</param>
        /// <param name="metadataPrefix">A <see cref="string"/> containing the metadata prefix.</param>
        /// <param name="properties">A <see cref="PropertyInfo[]"/>.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        protected string[] Serialize(IEnumerable<object> metadata, string metadataPrefix, PropertyInfo[] properties)=>
            metadata
                .Select(item => ConverterHelper.AsStrings(item, properties).ToArray())
                .Where(values => values.Any())
                .Select(values => values.AsCsv(metadataPrefix, this.dataTypeName))
                .ToArray();
    }
}
