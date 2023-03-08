using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Tests.ConverterTests
{
    [TestClass]
    public class ConverterConversionTests : ConverterBaseTests
    {
        [TestMethod]
        #region Test data
        [DataRow(true, DisplayName = "Enabled")]
        [DataRow(false, DisplayName = "Disabled")]
        #endregion
        public void TypelessValueConversionTest(bool conversionEnabled)
        {
            // Arrange
            var text = @"
ConversionValue,""Bread"",""Loaf""

Properties,FooBar,Id,Name
Values,FooBar,#,""Bread""
Values,FooBar,#,""Cake""
";
            var options =
                new Options()
                    .ForType("FooBar", "Id", "Name")
                    .ConversionsEnable(conversionEnabled);
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize");

            Assert.IsTrue(data.ContainsKey("FooBar"), "Failed to deserialize FooBar.");

            const string UNEXPECTED_VALUE = "Unexpected {0} of FooBar {1}.";
            var values = data["FooBar"].Item2.ToList();
            Assert.AreEqual(2, values.Count(), "Unexpected number of items of FooBar.");
            Assert.AreEqual(conversionEnabled ? "Loaf" : "Bread", values[0][1], UNEXPECTED_VALUE, "Name", 0);
            Assert.AreEqual("Cake", values[1][1], UNEXPECTED_VALUE, "Name", 1);
        }

        [TestMethod]
        #region Test data
        [DataRow(true, DisplayName = "Enabled")]
        [DataRow(false, DisplayName = "Disabled")]
        #endregion
        public void TypedValueConversionTest(bool conversionEnabled)
        {
            // Arrange
            var text = @"
ConversionValue,""Bread"",""Loaf""

Properties,Foo,Id,Name
Values,Foo,#,""Bread""
Values,Foo,#,""Cake""
";
            var options =
                new Options()
                    .ForType<Foo>()
                    .ConversionsEnable(conversionEnabled);
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize.");

            const string UNEXPECTED_VALUE = "Unexpected {0} of Foo {1}.";
            var fooData =
                data.Cast<Foo>()
                    .NotNull()
                    .ToList();
            Assert.AreEqual(2, fooData.Count(), "Unexpected number of items of Foo.");
            Assert.AreEqual(conversionEnabled ? "Loaf" : "Bread", fooData[0].Name, UNEXPECTED_VALUE, "Name", 0);
            Assert.AreEqual("Cake", fooData[1].Name, UNEXPECTED_VALUE, "Name", 1);
        }

        [TestMethod]
        #region Test data
        [DataRow(true, DisplayName = "Enabled")]
        [DataRow(false, DisplayName = "Disabled")]
        #endregion
        public void TypelessTypenameConversionTest(bool conversionEnabled)
        {
            var text = @"
ConversionType,""FooBar"",""FooBarBaz""

Properties,FooBar,Id,Name
Values,FooBar,#,""Bread""
Values,FooBar,#,""Cake""
";
            var options =
                new Options()
                    .ForType("FooBar", "Id", "Name")
                    .ConversionsEnable(conversionEnabled);
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize");

            Assert.IsTrue(data.ContainsKey(conversionEnabled ? "FooBarBaz" : "FooBar"), "Failed to deserialize FooBar.");

            const string UNEXPECTED_VALUE = "Unexpected {0} of FooBar {1}.";
            var values = data[conversionEnabled ? "FooBarBaz" : "FooBar"].Item2.ToList();
            Assert.AreEqual(2, values.Count, "Unexpected number of values.");
            Assert.AreEqual("Bread", values[0][1], UNEXPECTED_VALUE, "Name", 0);
            Assert.AreEqual("Cake", values[1][1], UNEXPECTED_VALUE, "Name", 1);
        }

        [TestMethod]
        #region Test data
        [DataRow(true, DisplayName = "Enabled")]
        [DataRow(false, DisplayName = "Disabled")]
        #endregion
        public void MetadataValueConversionTest(bool conversionEnabled)
        {
            // Arrange
            var text = @"
ConversionValue,Thing,OtherThing
ConversionValue,""Quoted"",""ConvertedQuotes""

Metadata,FooBar,Thing
Metadata,FooBar,""Thing""
Metadata,FooBar,Quoted
Metadata,FooBar,""Quoted""
Properties,FooBar,Id,Value
Values,FooBar,#,""One""
";
            var options =
                new Options()
                    .ForType("FooBar", "Id", "Value")
                    .ForMetadata("Metadata", true, "Name")
                    .ConversionsEnable(conversionEnabled);
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize data.");
            Assert.IsNotNull(converter.Metadata, "Failed to deserialize metadata.");

            Assert.IsTrue(converter.Metadata.ContainsKey("FooBar"), "Found no metadata for FooBar.");

            Assert.AreEqual(4, converter.Metadata["FooBar"].Count, "Unexpected number of metadata items for FooBar.");

            var foobarMetadata =
                converter.Metadata["FooBar"]
                    .Cast<Dictionary<string, string>>()
                    .ToList();
            Assert.IsNotNull(foobarMetadata, "Failed to extract metadata for FooBar.");

            Assert.IsTrue(foobarMetadata.All(md => md.ContainsKey("Name")), "Missing key for Name in Foobar metadata.");

            var metadataValues =
                foobarMetadata
                    .Select(md => md["Name"])
                    .ToList();

            const string MISSING_METADATA = "FooBar Metadata missing '{0}'.";
            if (conversionEnabled)
            {
                Assert.AreEqual(2, metadataValues.Count(md => md == "OtherThing"), MISSING_METADATA, "OtherThing");
                Assert.AreEqual(2, metadataValues.Count(md => md == "ConvertedQuotes"), MISSING_METADATA, "ConvertedQuotes");
            }
            else
            {
                Assert.AreEqual(2, metadataValues.Count(md => md == "Thing"), MISSING_METADATA, "Thing");
                Assert.AreEqual(2, metadataValues.Count(md => md == "Quoted"), MISSING_METADATA, "Quoted");
            }
        }
    }
}
