using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal abstract class BaseConversionData : BaseDeserializationData
    {
        protected BaseConversionData(DeserializationFactory factory)
            : base(factory) { }

        /// <summary>
        /// Gets the items that have the specified <paramref name="prefix"/>.
        /// </summary>
        /// <param name="prefix">A <see cref="string"/> that contains the prefix.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="string[]"/>.</returns>
        protected IEnumerable<string[]> GetItems(string prefix)
        {
            var lines =
                this.factory.Lines
                    .Where(line => line.StartsWith(prefix));
            var items =
                ConverterHelper.GetItems(lines,
                                         rejoinSplitQuotes: true,
                                         trimItems: true,
                                         typeName: null,
                                         prefix);
            return items;
        }
    }

    internal abstract class BaseConversionData<TConversion> : BaseConversionData
        where TConversion : BaseConversion
    {
        private readonly List<TConversion> conversionData = new();

        /// <summary>
        /// Gets the conversion data.
        /// </summary>
        protected TConversion[] Conversions => this.conversionData.ToArray();

        protected BaseConversionData(DeserializationFactory factory)
            : base(factory) { }

        /// <inheritdoc/>
        public sealed override void Deserialize()
        {
            var prefix = GetPrefix();
            var items = GetItems(prefix);
            var conversions = GetConversions(items, prefix);
            this.conversionData.Clear();
            this.conversionData.AddRange(conversions);
        }

        /// <summary>
        /// Get the prefix.
        /// </summary>
        /// <returns></returns>
        protected abstract string GetPrefix();

        /// <summary>
        /// Gets the conversions from the specified <paramref name="items"/> that have the 
        /// specified <paramref name="prefix"/>.
        /// </summary>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the items.</param>
        /// <param name="prefix">A <see cref="string"/> containing the prefix.</param>
        /// <returns>A <typeparamref name="TConversion"/> array.</returns>
        protected abstract TConversion[] GetConversions(IEnumerable<string[]> items, string prefix);
    }
}
