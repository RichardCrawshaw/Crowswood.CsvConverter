using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Helpers;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstract class for serializing typed object data.
    /// </summary>
    /// <remarks>
    /// Use <see cref="TypedObjectData{TObject}"/> for actual instances.
    /// </remarks>
    internal abstract class TypedObjectData : BaseObjectData
    {
        /// <summary>
        /// Gets the type that the current instance serializes.
        /// </summary>
        public abstract Type Type { get; }

        protected TypedObjectData(Serialization.SerializationFactory factory, string typeName)
            : base(factory, typeName) { }
    }

    /// <summary>
    /// A sealed class for serializing data of <typeparamref name="TObject"/>.
    /// </summary>
    /// <typeparam name="TObject">The type of the data.</typeparam>
    internal sealed class TypedObjectData<TObject> : TypedObjectData
        where TObject : class
    {
        private readonly IEnumerable<TObject> data;

        /// <inheritdoc/>
        protected override string[] Names => 
            typeof(TObject)
                .GetReadWriteProperties()
                .GetPropertyAndAttributePairs()
                .GetPropertyAndNamePairs()
                .Select(item => item.Name)
                .ToArray();

        /// <inheritdoc/>
        public override Type Type => typeof(TObject);

        public TypedObjectData(Serialization.SerializationFactory factory, string typeName, IEnumerable<TObject> data)
            : base(factory, typeName)
        {
            this.data = data;
            //this.properties = GetProperties(typeof(TObject));
            //this.Names = GetNames(this.properties);
        }

        /// <inheritdoc/>
        protected override string[] GetValues(string valuePrefix)
        {
            var properties = typeof(TObject).GetReadWriteProperties();

            var results =
                this.data
                    .Select(item => item.AsStrings(properties))
                    .Select(item => ConverterHelper.AsStrings(item))
                    .Select(values => values.ToArray())
                    .Where(values => values.Any())
                    .Where(values => values.Length == this.Names.Length)
                    .Select(values => values.AsCsv(valuePrefix, this.typeName))
                    .ToArray();
            return results;
        }
    }
}
