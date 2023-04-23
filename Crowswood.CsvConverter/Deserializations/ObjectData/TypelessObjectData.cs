using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Helpers;

namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class TypelessObjectData : BaseObjectData
    {
        /// <inheritdoc/>
        public override string ObjectTypeName { get; }

        internal TypelessObjectData(ObjectData objectData)
            : base(objectData) => this.ObjectTypeName = objectData.ObjectTypeName;

        /// <summary>
        /// Gets the deserialized object data as typeless object data.
        /// </summary>
        /// <returns>A tuple of <see cref="string[]"/> containing the property names and <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the values.</returns>
        public (string[] Names, IEnumerable<string[]> Values) GetData() =>
            this.propertyNames is null 
            ? (Array.Empty<string>(), Enumerable.Empty<string[]>())
            : (base.propertyNames, this.lazyValues.Value.Trim().Trim('"'));

        /// <summary>
        /// Determines and returns the type name following any type conversion process and type 
        /// attribute type name lookup.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        private string GetConveretedTypeName() =>
            ConversionHelper.ConvertType(
                this.ObjectTypeName,
                this.factory.Options.IsTypeConversionEnabled,
                this.factory.ConversionTypes);
    }
}
