using Crowswood.CsvConverter.Helpers;
using static Crowswood.CsvConverter.Deserialization;

namespace Crowswood.CsvConverter.Deserializations
{
    internal abstract class BaseConversionData : BaseDeserializationData
    {
        protected BaseConversionData(DeserializationFactory factory)
            : base(factory) { }

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
}
