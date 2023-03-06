using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class ConversionTests
    {
        [TestMethod]
        public void ConversionTypeTest()
        {
            // Arrange
            var original = "Original";
            var converted = "Converted";

            // Act
            var conversionType =
                new ConversionType
                {
                    OriginalTypeName = original,
                    ConvertedTypeName = converted
                };

            // Assert
            Assert.IsNotNull(conversionType, "Failed to create ConversionType instance.");

            Assert.AreEqual(original, conversionType.OriginalTypeName, "Unexpected original type name.");
            Assert.AreEqual(converted, conversionType.ConvertedTypeName, "Unexpected converted type name.");
        }

        [TestMethod]
        public void ConversionValueTest()
        {
            // Arrange
            var original = "Original";
            var converted = "Converted";

            // Act
            var conversionValue =
                new ConversionValue
                {
                    OriginalValue = original,
                    ConvertedValue = converted
                };

            // Assert
            Assert.IsNotNull(conversionValue, "Failed to create ConversionType instance.");

            Assert.AreEqual(original, conversionValue.OriginalValue, "Unexpected original value.");
            Assert.AreEqual(converted, conversionValue.ConvertedValue, "Unexpected converted value.");
        }

        [TestMethod]
        public void HandlerConstructorTest()
        {
            // Arrange
            var text = @"
ConversionType,OriginalType,NewType
ConversionValue,Foo,Bar
";
            var options = Options.None;
            var configHandler = new ConfigHandler(Options.None);
            var lines = 
                text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            // Act
            var conversionHandler = new ConversionHandler(options, configHandler, lines);

            // Assert
            Assert.IsNotNull(conversionHandler, "Failed to create ConversionHandler.");

            Assert.AreEqual(1, conversionHandler.ConversionTypes.Length, "Unexpected number of ConversionTypes.");
            Assert.AreEqual(1, conversionHandler.ConversionValues.Length, "Unexpected number of ConversionValues.");

            var conversionType = conversionHandler.ConversionTypes[0];
            Assert.AreEqual("OriginalType", conversionType.OriginalTypeName, "Unexpected ConversionType OriginalType.");
            Assert.AreEqual("NewType", conversionType.ConvertedTypeName, "Unexpected ConversionType ConvertedType.");

            var conversionValue = conversionHandler.ConversionValues[0];
            Assert.AreEqual("Foo", conversionValue.OriginalValue, "Unexpected ConversionValue OriginalValue.");
            Assert.AreEqual("Bar", conversionValue.ConvertedValue, "Unexpected ConversionValue ConvertedValue.");
        }

        [TestMethod]
        public void OptionsNoneTest()
        {
            // Arrange

            // Act
            var options = Options.None;

            // Assert
            const string UNEXPECTED_STATE = "Unexpected {0} enabled state.";
            const string UNEXPECTED_PREFIX = "Unexpected {0} prefix.";

            Assert.AreEqual(false, options.IsTypeConversionEnabled, UNEXPECTED_STATE, "ConversionType");
            Assert.AreEqual(false, options.IsValueConversionEnabled, UNEXPECTED_STATE, "ConversionValue");

            Assert.AreEqual("ConversionType", options.ConversionTypePrefix, UNEXPECTED_PREFIX, "ConversionType");
            Assert.AreEqual("ConversionValue", options.ConversionValuePrefix, UNEXPECTED_PREFIX, "ConversionValue");
        }

        [TestMethod]
        #region Test data
        [DataRow(false, DisplayName = "Disabled")]
        [DataRow(true, DisplayName = "Enabled")]
        #endregion
        public void OptionsEnabledTogetherTest(bool enabledFlag)
        {
            // Arrange

            // Act
            var options =
                new Options()
                    .ConversionsEnable(enabledFlag);

            // Assert
            const string UNEXPECTED_STATE = "Unexpected {0} enabled state.";

            Assert.AreEqual(enabledFlag, options.IsTypeConversionEnabled, UNEXPECTED_STATE, "ConversionType");
            Assert.AreEqual(enabledFlag, options.IsValueConversionEnabled, UNEXPECTED_STATE, "ConversionValue");
        }

        [TestMethod]
        #region Test data
        [DataRow(false, false, DisplayName = "Both disabled")]
        [DataRow(false, true, DisplayName = "Type enabled only")]
        [DataRow(true, false, DisplayName = "Value enabled only")]
        [DataRow(true, true, DisplayName = "Both enabled")]
        #endregion
        public void OptionsEnabledIndividualyTest(bool typeEnabledFlag, bool valueEnabledFlag)
        {
            // Arrange

            // Act
            var options =
                new Options()
                    .ConversionsEnable(typeEnabledFlag, valueEnabledFlag);

            // Assert
            const string UNEXPECTED_STATE = "Unexpected {0} enabled state.";

            Assert.AreEqual(typeEnabledFlag, options.IsTypeConversionEnabled, UNEXPECTED_STATE, "ConversionType");
            Assert.AreEqual(valueEnabledFlag, options.IsValueConversionEnabled, UNEXPECTED_STATE, "ConversionValue");
        }

        [TestMethod]
        public void OptionsPrefixesTest()
        {
            // Arrange
            const string UNEXPECTED_PREFIX = "Unexpected {0} prefix.";

            // Act
            var options =
                new Options()
                    .SetPrefixes(conversionTypePrefix: "Foo",
                                 conversionValuePrefix: "Bar");

            // Assert
            Assert.AreEqual("Foo", options.ConversionTypePrefix, UNEXPECTED_PREFIX, "ConversionType");
            Assert.AreEqual("Bar", options.ConversionValuePrefix, UNEXPECTED_PREFIX, "ConversionValue");
        }

        [TestMethod]
        public void HandlerConversionDisabledTest()
        {
            // Arrange
            var text = @"
ConversionType,OriginalType,NewType
ConversionValue,Foo,Bar
";
            var options = Options.None;
            var configHandler = new ConfigHandler(options);
            var lines =
                text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var conversionHandler = new ConversionHandler(options, configHandler, lines);

            // Act
            var convertedTypeText = conversionHandler.ConvertType("OriginalType");
            var convertedValueText = conversionHandler.ConvertValue("Foo");

            // Assert
            Assert.AreEqual("OriginalType", convertedTypeText, "Failed to convert type.");
            Assert.AreEqual("Foo", convertedValueText, "Failed to convert value.");
        }

        [TestMethod]
        public void HandlerConversionEnabledTest()
        {
            // Arrange
            var text = @"
ConversionType,OriginalType,NewType
ConversionValue,Foo,Bar
";
            var options =
                new Options()
                    .ConversionsEnable(true);
            var configHandler = new ConfigHandler(options);
            var lines =
                text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var conversionHandler = new ConversionHandler(options, configHandler, lines);

            // Act
            var convertedTypeText = conversionHandler.ConvertType("OriginalType");
            var convertedValueText = conversionHandler.ConvertValue("Foo");

            // Assert
            Assert.AreEqual("NewType", convertedTypeText, "Failed to convert type.");
            Assert.AreEqual("Bar", convertedValueText, "Failed to convert value.");
        }
    }
}
