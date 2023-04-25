using Crowswood.CsvConverter.Handlers;
using Crowswood.CsvConverter.Model;

namespace Crowswood.CsvConverter.Tests.Internal
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
