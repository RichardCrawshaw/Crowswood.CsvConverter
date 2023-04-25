namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class OptionsTests
    {
        [TestMethod]
        public void ConstructorTest()
        {
            // Act
            var options = new Options();

            // Assert
            Assert.IsNotNull(options, "Failed to create CSV Converter Options.");
        }

        [TestMethod]
        public void CommentPrefixesTest()
        {
            // Arrange
            var options = new Options();

            // Assert
            const string failureMessage = "Comment characters does not contain {0}.";
            Assert.IsTrue(options.CommentPrefixes.Contains("!"), failureMessage, "exclamation mark");
            Assert.IsTrue(options.CommentPrefixes.Contains("#"), failureMessage, "hash");
            Assert.IsTrue(options.CommentPrefixes.Contains(";"), failureMessage, "semi-colon");
            Assert.IsTrue(options.CommentPrefixes.Contains("//"), failureMessage, "double slash");
            Assert.IsTrue(options.CommentPrefixes.Contains("--"), failureMessage, "double dash");
        }

        [TestMethod]
        public void PropertyAndValuesPrefixesTest()
        {
            // Arrange
            var options = new Options();

            // Assert
            Assert.AreEqual("Properties", options.PropertyPrefix, "Unexpected property prefix.");
            Assert.AreEqual("Values", options.ValuesPrefix, "Unexpected values prefix.");
        }

        [TestMethod]
        public void PrefixesCanBeChangedTest()
        {
            // Arrange
            var options = new Options();

            // Act
            options.SetPrefixes(propertiesPrefix: "Foo",
                                valuesPrefix: "Bar");
            options.CommentPrefixes =
                new[]
                {
                    "'",
                };

            // Assert
            Assert.AreEqual("Foo", options.PropertyPrefix, "Unexpected property prefix.");
            Assert.AreEqual("Bar", options.ValuesPrefix, "Unexpected values prefix.");
            Assert.AreEqual(1, options.CommentPrefixes.Length, "Unexpected number of comment prefixes.");
            Assert.IsTrue(options.CommentPrefixes.Contains("'"), "Unexpected comment prefix.");
        }
    }
}
