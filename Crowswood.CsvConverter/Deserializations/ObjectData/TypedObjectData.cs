using Crowswood.CsvConverter.Exceptions;
using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Model;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    /// <summary>
    /// A base abstract class that is the base object for all typed object data classes.
    /// </summary>
    internal abstract class BaseTypedObjectData : BaseObjectData
    {
        private readonly Lazy<IEnumerable<PropertyAndNamePair>> lazyPropertyAndNamePairs;
        private readonly Lazy<string> lazyTypeName;

        /// <inheritdoc/>
        public override string ObjectTypeName => this.lazyTypeName.Value;

        /// <summary>
        /// Get the property and name pairs for the object type.
        /// </summary>
        protected IEnumerable<PropertyAndNamePair> PropertyAndNamePairs => this.lazyPropertyAndNamePairs.Value;

        /// <summary>
        /// Gets the <see cref="Type"/> of the object data.
        /// </summary>
        public abstract Type Type { get; }

        protected BaseTypedObjectData(DeserializationFactory factory)
            : base(factory)
        {
            this.lazyTypeName = new(() => this.Type.GetTypeName());
            this.lazyPropertyAndNamePairs =
                new(() =>
                    this.Type
                        .GetPropertyAndAttributePairs()
                        .GetPropertyAndNamePairs());
        }

        protected BaseTypedObjectData(ObjectData source)
            : base(source)
        {
            this.lazyTypeName = new(() => this.Type.GetTypeName());
            this.lazyPropertyAndNamePairs =
                new(() =>
                    this.Type
                        .GetPropertyAndAttributePairs()
                        .GetPropertyAndNamePairs());
        }

        /// <summary>
        /// Get the data for the object.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> of <typeparamref name="TObject"/>.</returns>
        /// <exception cref="DataMustBeDeserializedException">If the data has not been deserialized.</exception>
        protected IEnumerable<TObject> GetData<TObject>() where TObject : class
        {
            if ((this.propertyNames is null) || (this.values is null))
                throw new DataMustBeDeserializedException();

            var results = new List<TObject>();

            foreach(var values in this.values)
            {
                var value = SetValues<TObject>(this.PropertyNames, values);
                results.Add(value);
            }

            return results;
        }

        /// <inheritdoc/>
        protected override void PrepareMetadata()
        {
            base.PrepareMetadata();
            this.metadata.AddRange(
                this.Type.GetAttributes()
                    .Select(attr =>
                        new TypedMetadataData(factory.Clone(),
                                              this.ObjectTypeName,
                                              attr.GetType())));
        }

        /// <summary>
        /// Sets the <paramref name="values"/> on a new instance of <typeparamref name="T"/> using 
        /// the specified <paramref name="propertyNames"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of object to be returned.</typeparam>
        /// <param name="propertyNames">A <see cref="string[]"/> containing the property names.</param>
        /// <param name="values">A <see cref="string[]"/> containing the values.</param>
        /// <returns>A <typeparamref name="T"/>.</returns>
        protected T SetValues<T>(string[] propertyNames, string[] values)
            where T : class =>
            (T)SetValues(this.Type, propertyNames, values, this.factory.Options.OptionMembers, this.PropertyAndNamePairs);
    }

    /// <summary>
    /// A sealed non-generic class that describes a typed object data instance.
    /// </summary>
    internal sealed class TypedObjectData : BaseTypedObjectData
    {
        /// <inheritdoc/>
        public override Type Type { get; }

        public TypedObjectData(DeserializationFactory factory, Type objectType)
            : base(factory) => this.Type = objectType;

        public TypedObjectData(ObjectData source, Type objectType)
            : base(source) => this.Type = objectType;

        /// <inheritdoc/>
        public IEnumerable<object> GetData() => GetData<object>();
    }

    /// <summary>
    /// A sealed generic class that describes a typed object data instance.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    internal sealed class TypedObjectData<TObject> : BaseTypedObjectData
        where TObject : class, new()
    {
        /// <inheritdoc/>
        public override Type Type => typeof(TObject);

        public TypedObjectData(DeserializationFactory factory)
            : base(factory) { }

        /// <inheritdoc/>
        public IEnumerable<TObject> GetData() => GetData<TObject>();
    }
}
