using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// An abstract base class for serializing object data.
    /// </summary>
    internal abstract class BaseObjectData : BaseSerializationData
    {
        protected readonly string typeName;
        protected readonly Serialization.SerializationFactory factory;

        /// <summary>
        /// Gets the names.
        /// </summary>
        protected abstract string[] Names { get; }

        protected BaseObjectData(Serialization.SerializationFactory factory, string typeName)
        {
            this.factory = factory;
            this.typeName = typeName;
        }

        /// <inheritdoc/>
        public override string[] Serialize() => Serialize(GetValuePrefix());

        /// <summary>
        /// Gets the values using the specified <paramref name="valuePrefix"/> with one element 
        /// per record.
        /// </summary>
        /// <param name="valuePrefix">A <see cref="string"/> containing the value prefix.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        protected abstract string[] GetValues(string valuePrefix);

        /// <summary>
        /// Gets the value prefix.
        /// </summary>
        protected string GetValuePrefix() => this.factory.GetvaluePrefix(this.typeName);

        /// <summary>
        /// Gets the names as a <see cref="string[]"/> with only one element.
        /// </summary>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] GetNames() => GetNames(this.factory.GetPropertyPrefix(this.typeName));

        /// <summary>
        /// Gets the names as a string with only one element using the specified <paramref name="propertyPrefix"/>.
        /// </summary>
        /// <param name="propertyPrefix">A <see cref="string"/> containing the property prefix.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] GetNames(string propertyPrefix) =>
            new[] { this.Names.AsCsv(propertyPrefix, this.typeName), }; 

        /// <summary>
        /// Serialize the data using the specified <paramref name="valuePrefix"/>.
        /// </summary>
        /// <param name="valuePrefix">A <see cref="string"/> containing the value prefix.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private string[] Serialize(string valuePrefix) =>
            new List<string[]> { GetNames(), GetValues(valuePrefix), }
                .SelectMany(items => items)
                .ToArray();
    }
}
