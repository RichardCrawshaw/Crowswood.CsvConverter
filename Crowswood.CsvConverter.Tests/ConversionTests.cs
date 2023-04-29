namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class ConversionTests
    {
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
    }
}
