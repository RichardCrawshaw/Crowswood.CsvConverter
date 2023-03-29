using System.Reflection;
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

        /// <summary>
        /// Creates and returns an instance of <see cref="TypedMetadataData{TMetadata}"/> for the 
        /// specified <paramref name="metadataType"/> using the specified <paramref name="serialization"/> 
        /// and <paramref name="dataTypeName"/> that serializes the specified <paramref name="metadata"/>.
        /// </summary>
        /// <param name="metadataType">A <see cref="Type"/> that defines the type of the metadata.</param>
        /// <param name="serialization">The <see cref="Serialization"/> instance that the new instance will belong to.</param>
        /// <param name="dataTypeName">A <see cref="string"/> that contains the name of the data object type.</param>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <see cref="object"/> that contains the metadata; each element must be able to be assigned to <paramref name="metadataType"/>.</param>
        /// <returns>A <see cref="TypedMetadataData"/> instance.</returns>
        /// <exception cref="InvalidOperationException">If an instance cannot be created.</exception>
        /// <remarks>
        /// Calls <seealso cref="Create{TMetadata}(Serialization.SerializationFactory, string, IEnumerable{object})"/>
        /// by reflection.
        /// </remarks>
        internal static TypedMetadataData Create(Type metadataType,
                                                 Serialization serialization,
                                                 string dataTypeName,
                                                 IEnumerable<object> metadata)
        {
            var parameters =
                new object[]
                {
                    new Serialization.SerializationFactory(serialization),
                    dataTypeName,
                    metadata,
                };

            var method =
                typeof(TypedMetadataData).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    .Where(method => method.Name == "Create")
                    .Where(method => method.IsGenericMethod)
                    .SingleOrDefault()?.MakeGenericMethod(metadataType);

            var result =
                method?.Invoke(null, parameters) ??
                throw new InvalidOperationException(
                    $"Failed to bind to {nameof(TypedMetadataData)}<{metadataType.Name}>.");

            return (TypedMetadataData)result;
        }

        /// <summary>
        /// Creates and returns an instance of <see cref="TypedMetadataData{TMetadata}"/> for the 
        /// specified <typeparamref name="TMetadata"/> using the specified <paramref name="factory"/>, 
        /// and <paramref name="dataTypeName"/> that serializes the specified <paramref name="metadata"/>.
        /// </summary>
        /// <typeparam name="TMetadata">The type of the metadata.</typeparam>
        /// <param name="factory">A <see cref="Serialization.SerializationFactory"/> object.</param>
        /// <param name="dataTypeName">A <see cref="string"/> that contains the name of the data object type.</param>
        /// <param name="metadata">An <see cref="IEnumerable{T}"/> of <see cref="object"/> that contains the metadata; each element must be able to be assigned to <paramref name="metadataType"/>.</param>
        /// <returns>A <see cref="TypedMetadataData"/> instance.</returns>
        /// <remarks>
        /// Method is called using reflection by <seealso cref="Create(Type, Serialization, string, IEnumerable{object})"/>
        /// </remarks>
        private static TypedMetadataData Create<TMetadata>(Serialization.SerializationFactory factory,
                                                           string dataTypeName,
                                                           IEnumerable<object> metadata)
            where TMetadata : class
        {
            var typedMetadata =
                metadata
                    .Where(md => md.GetType().IsAssignableTo(typeof(TMetadata)))
                    .Cast<TMetadata>()
                    .ToList();

            var result =
                new TypedMetadataData<TMetadata>(factory, dataTypeName, typedMetadata);
            return result;
        }
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
        public override string[] Serialize() =>
            Serialize(
                this.factory.Options.GetOptionMetadata(this.MetadataType) ??
                throw new InvalidOperationException(
                    $"No Metadata definition in Options for '{this.MetadataTypeName}'."));

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> from <typeparamref name="T"/> that exist in the 
        /// specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="MetadataType"/> to get the properties of.</typeparam>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the property names.</param>
        /// <returns>A <see cref="PropertyInfo[]"/>.</returns>
        private static PropertyInfo[] GetProperties<T>(string[] propertyNames) => 
            GetProperties(typeof(T), propertyNames);

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> from the specified <paramref name="properties"/> 
        /// according to the order defined in the specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="properties">A <see cref="PropertyInfo[]"/>.</param>
        /// <param name="propertyNames">A <see cref="string[]"/> contains the names of the property names.</param>
        /// <returns>A <see cref="PropertyInfo[]"/>.</returns>
        /// <remarks>
        /// The values must be in the order defined in the Options Metadata, so derive them from 
        /// the PropertyNames, rather than directly from the properties of the Type.
        /// </remarks>
        private static PropertyInfo[] GetProperties(PropertyInfo[] properties, string[] propertyNames) =>
            propertyNames
                .Select(propertyName =>
                    properties
                        .FirstOrDefault(property => property.Name == propertyName))
                .NotNull()
                .ToArray();

        /// <summary>
        /// Serializes the <seealso cref="metadata"/> using the specified <paramref name="optionMetadata"/> 
        /// </summary>
        /// <param name="optionMetadata"></param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] Serialize(OptionMetadata optionMetadata) =>
            Serialize(optionMetadata, GetProperties<TMetadata>(optionMetadata.PropertyNames));

        /// <summary>
        /// Serializes the <seealso cref="metadata"/> using the specified <paramref name="optionMetadata"/> 
        /// and <paramref name="properties"/>.
        /// </summary>
        /// <param name="optionMetadata"></param>
        /// <param name="properties">A <see cref="PropertyInfo[]"/>.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] Serialize(OptionMetadata optionMetadata, PropertyInfo[] properties) =>
            Serialize(this.metadata,
                      optionMetadata.Prefix,
                      GetProperties(properties, optionMetadata.PropertyNames));
    }
}
