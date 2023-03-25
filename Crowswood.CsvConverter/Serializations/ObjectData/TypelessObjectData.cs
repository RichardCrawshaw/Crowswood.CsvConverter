using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for serializing typeless data.
    /// </summary>
    internal sealed class TypelessObjectData : BaseObjectData
    {
        private readonly IEnumerable<string[]> values;

        /// <inheritdoc/>
        protected override string[] Names { get; }

        public TypelessObjectData(Serialization.SerializationFactory prefixFactory, string typeName, string[] names, IEnumerable<string[]> values)
            : base(prefixFactory, typeName)
        {
            this.Names = names;
            this.values = values;
        }

        /// <inheritdoc/>
        protected override string[] GetValues(string valuePrefix) =>
            this.values
                .Select(values => GetValues(values))
                .Select(values => values.AsCsv(valuePrefix, this.typeName))
                .ToArray();

        /// <summary>
        /// Gets the specified <paramref name="values"/> as double-quoted strings.
        /// </summary>
        /// <param name="values">A <see cref="string[]"/> containing the values.</param>
        /// <returns>A <see cref="string[]"/>.</returns>
        private static string[] GetValues(string[] values) =>
            values
                .Select(value => $"\"{value}\"")
                .ToArray();
    }
}
