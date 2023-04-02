using Crowswood.CsvConverter.Exceptions;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Model;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    /// <summary>
    /// A base abstract class for typed metadata.
    /// </summary>
    internal abstract class BaseTypedMetadataData : BaseMetadataData
    {
        #region Fields

        private readonly Lazy<IEnumerable<PropertyAndNamePair>> lazyPropertyAndNamePairs;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="Type"/> of the metadata.
        /// </summary>
        public abstract Type MetadataType { get; }

        /// <summary>
        /// Gets the <see cref="PropertyAndNamePair"/> objects.
        /// </summary>
        protected IEnumerable<PropertyAndNamePair> PropertyAndNamePairs => this.lazyPropertyAndNamePairs.Value;

        #endregion

        #region Constructors

        protected BaseTypedMetadataData(DeserializationFactory factory, string objectTypeName, string metadataTypeName)
            : base(factory, objectTypeName, metadataTypeName) =>
            this.lazyPropertyAndNamePairs =
                new(() =>
                    this.MetadataType
                        .GetPropertyAndAttributePairs()
                        .GetPropertyAndNamePairs());

        #endregion

        #region Support routines

        /// <summary>
        /// Get the metadata as a <see cref="List{T}"/> of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the metadata; will being either <see cref="object"/> or <seealso cref="MetadataType"/>.</typeparam>
        /// <returns>A <seealso cref="List{T}"/> of <typeparamref name="T"/>.</returns>
        /// <exception cref="MetadataNotDeserializedException">If the metadata has not been deserialized.</exception>
        protected List<T> GetMetadata<T>() where T : class =>
            this.propertyNames is not null && this.values is not null
                ? this.values
                    .Select(values => SetValues<T>(this.propertyNames, values))
                    .ToList()
                : throw new MetadataNotDeserializedException();

        /// <inheritdoc/>
        protected override OptionMetadata? GetOptionMetadata() =>
            this.factory.Options.GetOptionMetadata(this.MetadataType);

        /// <summary>
        /// Sets the <paramref name="values"/> on a new instance of <typeparamref name="T"/> using 
        /// the specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object to be returned.</typeparam>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the property names.</param>
        /// <param name="values">A <see cref="string[]"/> containing the values.</param>
        /// <returns>A <typeparamref name="T"/>.</returns>
        protected T SetValues<T>(string[] propertyNames, string[] values) where T : class =>
            (T)SetValues(this.MetadataType, propertyNames, values, this.factory.Options.OptionMembers, this.PropertyAndNamePairs);

        #endregion
    }

    /// <summary>
    /// A sealed non-generic class for typed metadata.
    /// </summary>
    internal sealed class TypedMetadataData : BaseTypedMetadataData
    {
        /// <inheritdoc/>
        public override Type MetadataType { get; }

        public TypedMetadataData(DeserializationFactory factory, string objectTypeName, Type metadataType)
            : base(factory, objectTypeName, metadataType.GetTypeName()) => this.MetadataType = metadataType;

        /// <summary>
        /// Gets the metadata as a <see cref="List{T}"/> of <see cref="object"/>.
        /// </summary>
        /// <returns></returns>
        public List<object> GetMetadata() => GetMetadata<object>();
    }

    /// <summary>
    /// A sealed generic class for typed metadata.
    /// </summary>
    /// <typeparam name="TMetadata"></typeparam>
    internal sealed class TypedMetadataData<TMetadata> : BaseTypedMetadataData
        where TMetadata : class, new()
    {
        /// <inheritdoc/>
        public override Type MetadataType => typeof(TMetadata);

        public TypedMetadataData(DeserializationFactory factory, string objectTypeName)
            : base(factory, objectTypeName, typeof(TMetadata).Name) { }

        /// <summary>
        /// Gets the metadata as a <see cref="List{T}"/> of <typeparamref name="TMetadata"/>.
        /// </summary>
        /// <returns>The metadata as a <see cref="List{T}"/> of <typeparamref name="TMetadata"/>.</returns>
        public List<TMetadata> GetMetadata() => GetMetadata<TMetadata>();
    }
}
