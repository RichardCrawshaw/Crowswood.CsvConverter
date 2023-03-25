namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// A sealed class for holding blank line data.
    /// </summary>
    internal sealed class BlankLinesData : BaseSerializationData
    {
        private readonly int number;

        public BlankLinesData(int number) => this.number = number;

        /// <inheritdoc/>
        public override string[] Serialize() =>
            Enumerable
                .Range(0, this.number)
                .Select(_ => string.Empty)
                .ToArray();
    }

}
