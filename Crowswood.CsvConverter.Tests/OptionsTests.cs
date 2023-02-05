﻿namespace Crowswood.CsvConverter.Tests
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
        public void InitialNoneTest()
        {
            // Act
            var options = Options.None;

            // Assert
            Assert.IsNotNull(options, "Failed to generate 'Options.None'.");
            Assert.AreEqual(0, options.OptionMembers.Length, "There should be no 'OptionMembers'.");
            Assert.AreEqual(0, options.OptionMetadata.Length, "There should be no 'OptionsMetadata'.");
            Assert.AreEqual(0, options.OptionTypes.Length, "There should be no 'OptionsTypes'.");
        }

        [TestMethod]
        public void ModifyNoneTest()
        {
            // Arrange
            var options = Options.None;

            // Act
            options.ForMember<Foo, int>(foo => foo.Id, "ID");
            options.ForMetadata<Foo>("Foo", "Bar", "Baz");
            options.ForType<Foo>();

            // Assert
            Assert.IsNotNull(options, "Failed to generate 'Options.None'.");
            Assert.AreEqual(0, options.OptionMembers.Length, "There should be no 'OptionMembers'.");
            Assert.AreEqual(0, options.OptionMetadata.Length, "There should be no 'OptionsMetadata'.");
            Assert.AreEqual(0, options.OptionTypes.Length, "There should be no 'OptionsTypes'.");
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
            options.SetPrefixes("Foo", "Bar");
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

        [TestMethod]
        public void StandardUseTest()
        {
            // Arrange
            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMember<Foo, int>(foo => foo.Id, "ID")
                    .ForMetadata<Bar>("Foo", "Bar", "Baz");

            // Assert
            Assert.AreEqual(1, options.OptionTypes.Length, "Unexpected number of Types.");
            Assert.AreEqual(typeof(Foo), options.OptionTypes[0].Type, "Unexpected OptionTypes 0 Type.");

            Assert.AreEqual(1, options.OptionMembers.Length, "Unexpected number of Members.");
            Assert.AreEqual(typeof(Foo), options.OptionMembers[0].Type, "Unexpected OptionMembers 0 Type.");
            Assert.AreEqual("ID", options.OptionMembers[0].Name, "Unexpected OptionMembers 0 Name.");
            Assert.AreEqual(typeof(Foo).GetProperty("Id"), options.OptionMembers[0].Property, "Unexpected OptionMembers 0 Property.");

            Assert.AreEqual(1, options.OptionMetadata.Length, "Unexpected number of Metadata.");
            Assert.AreEqual(typeof(Bar), options.OptionMetadata[0].Type, "Unexpected OptionMetadata 0 Type.");
            Assert.AreEqual("Foo", options.OptionMetadata[0].Prefix, "Unexpected OptionMetadata 0 Prefix.");
            Assert.AreEqual(typeof(OptionMetadata<Bar>), options.OptionMetadata[0].GetType(), "Unexpected OptionMetadata 0 generic type.");
            Assert.IsTrue(
                ((OptionMetadata<Bar>)options.OptionMetadata[0]).PropertyNames
                    .GroupBy(propertyName => propertyName)
                    .ToDictionary(n => n.Key, n => n.Count())
                    .All(kvp => (kvp.Key == "Bar" && kvp.Value == 1) ||
                                (kvp.Key == "Baz" && kvp.Value == 1)),
                "Unexpected OptionMetadata 0 PropertyNames.");
        }

        private class Foo
        {
            public int Id { get; set; }
        }

        private class Bar { }
    }
}
