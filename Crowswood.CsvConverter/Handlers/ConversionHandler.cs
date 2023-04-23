using Crowswood.CsvConverter.Helpers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Handlers
{
    internal class ConversionHandler
    {
        #region Fields

        private readonly Options options;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="ConversionType"/> objects.
        /// </summary>
        public ConversionType[] ConversionTypes { get; }

        /// <summary>
        /// Gets the <see cref="ConversionValue"/> objects.
        /// </summary>
        public ConversionValue[] ConversionValues { get; }

        /// <summary>
        /// Gets whether the conversion of types is enabled.
        /// </summary>
        private bool IsTypeConversionEnabled => this.options.IsTypeConversionEnabled;

        /// <summary>
        /// Gets whether the conversion of values is enabled.
        /// </summary>
        private bool IsValueConversionEnabled => this.options.IsValueConversionEnabled;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance using the specified <paramref name="options"/> deriving the 
        /// <see cref="ConversionType"/> and <see cref="ConversionValue"/> arrays from the 
        /// specified <paramref name="configHandler"/> and <paramref name="lines"/>.
        /// </summary>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <param name="configHandler">A <see cref="ConfigHandler"/> instance.</param>
        /// <param name="lines">An <see cref="IEnumerable{T}"/> of <see cref="string"/> that contains the lines to parse for conversions.</param>
        public ConversionHandler(Options options, ConfigHandler configHandler, IEnumerable<string> lines)
            : this(options, 
                   configHandler, 
                   ConverterHelper.GetItems(lines,
                                            rejoinSplitQuotes: true,
                                            trimItems: true,
                                            typeName: null,
                                            configHandler.GetConversionTypePrefix(),
                                            configHandler.GetConversionValuePrefix()))
        { }

        /// <summary>
        /// Create a new instance using the specified <paramref name="options"/> deriving the 
        /// <see cref="ConversionType"/> and <see cref="ConversionValue"/> arrays from the 
        /// specified <paramref name="items"/>.
        /// </summary>
        /// <param name="options">An <see cref="Options"/> object.</param>
        /// <param name="configHandler"></param>
        /// <param name="items">An <see cref="IEnumerable{T}"/> of <see cref="string[]"/> containing the items to parse for conversions.</param>
        private ConversionHandler(Options options, ConfigHandler configHandler, IEnumerable<string[]> items)
            : this(options,
                   ConversionHelper.GetConversionTypes(items, configHandler),
                   ConversionHelper.GetConversionValues(items, configHandler))
        { }

        /// <summary>
        /// Creata a new instance using the specified <paramref name="options"/>, 
        /// <paramref name="conversionTypes"/> and <paramref name="conversionValues"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="conversionTypes"></param>
        /// <param name="conversionValues"></param>
        private ConversionHandler(Options options, ConversionType[] conversionTypes, ConversionValue[] conversionValues)
        {
            this.options = options;
            this.ConversionTypes = conversionTypes;
            this.ConversionValues = conversionValues;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the specified <paramref name="typeName"/>.
        /// </summary>
        /// <param name="typeName">A <see cref="string"/> containing the typename to convert.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public string ConvertType(string typeName) =>
            ConversionHelper.ConvertType(typeName, this.IsTypeConversionEnabled, this.ConversionTypes);

        /// <summary>
        /// Converts the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">A <see cref="string"/> containing the value to convert.</param>
        /// <returns>A <see cref="string"/>.</returns>
        public string ConvertValue(string value) =>
            ConversionHelper.ConvertValue(value, this.IsValueConversionEnabled, this.ConversionValues);

        #endregion
    }
}
