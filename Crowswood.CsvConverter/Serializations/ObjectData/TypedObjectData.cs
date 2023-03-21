using System.Reflection;
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
        private readonly PropertyInfo[] properties;

        /// <inheritdoc/>
        protected override string[] Names { get; }

        /// <inheritdoc/>
        public override Type Type => typeof(TObject);

        public TypedObjectData(Serialization.SerializationFactory factory, string typeName, IEnumerable<TObject> data)
            : base(factory, typeName)
        {
            this.data = data;
            this.properties = GetProperties(typeof(TObject));
            this.Names = GetNames(this.properties);
        }

        /// <inheritdoc/>
        protected override string[] GetValues(string valuePrefix) =>
            this.data
                .Select(item => TypedObjectData<TObject>.GetValues(item, this.properties))
                .Where(values => values.Any())
                .Where(values => values.Length == this.Names.Length)
                .Select(values => values.AsCsv(valuePrefix, this.typeName))
                .ToArray();

        /// <summary>
        /// Gets the names from the specified <paramref name="properties"/>.
        /// </summary>
        /// <param name="properties">A <see cref="PropertyInfo[]"/>.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private static string[] GetNames(PropertyInfo[] properties) =>
            properties
                .GetPropertyAndAttributePairs()
                .GetPropertyAndNamePairs()
                .Select(item => item.Name)
                .ToArray();

        /// <summary>
        /// Gets public instance properties from the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">A <see cref="Type"/>.</param>
        /// <returns>A <see cref="PropertyInfo[]"/>.</returns>
        private static PropertyInfo[] GetProperties(Type type) =>
            type.GetProperties(BindingFlags.Instance |
                               BindingFlags.Public)
                .Where(property => property.CanRead && property.CanWrite)
                .ToArray();

        /// <summary>
        /// Gets the property values of the specified <paramref name="item"/> using the specified 
        /// <paramref name="properties"/>.
        /// </summary>
        /// <param name="item">A <typeparamref name="TObject"/> object.</param>
        /// <param name="properties">A <see cref="PropertyInfo[]"/>.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private static string[] GetValues(TObject item, PropertyInfo[] properties) =>
            ConverterHelper.AsStrings(item, properties)
                .ToArray();
    }
}
