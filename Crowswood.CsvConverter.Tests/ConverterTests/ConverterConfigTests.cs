namespace Crowswood.CsvConverter.Tests.ConverterTests
{
    [TestClass]
    public class ConverterConfigTests
    {
        /// <summary>
        /// This test proves that the deserialization of data is not affected when there is 
        /// global / typed config that does not target deserialization. And that any old text can 
        /// be set as configuration data without generating exceptions.
        /// </summary>
        [TestMethod]
        public void ConfigurationRubbishTest()
        {
            // Assign
            var text = @"
GlobalConfig,ExampleName,ExampleValue
TypedConfig,TypeName,ExampleName,ExampleValue2

Properties,ExampleType,Id,Name,Value
Values,ExampleType,#,""Name One"",""Some value""
Values,ExampleType,#,""Name 2"",""Another value""
Values,ExampleType,#,""Third name"",""A further value""
";
            var options =
                new Options()
                    .ForType("ExampleType", "Id", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize text to typeless data.");

            Assert.IsTrue(data.ContainsKey("ExampleType"), "Expected data not deserialized.");

            var exampleTypeData = data["ExampleType"];
            Assert.IsNotNull(exampleTypeData, "Expected data is null.");
            Assert.AreEqual(3, exampleTypeData.Item1.Length, "Unexpected number of properties.");
            Assert.AreEqual(3, exampleTypeData.Item2.Count(), "Unexpected number of records.");
            Assert.IsTrue(exampleTypeData.Item2.All(n => n.Length == 3), "The records have variable number of fields.");
        }

        /// <summary>
        /// This test proves that the global config works for data deserialzation.
        /// </summary>
        [TestMethod]
        public void ConfigurationGlobalTest()
        {
            // Assign
            var text = @"
GlobalConfig,PropertyPrefix,PP
GlobalConfig,ValuesPrefix,VP

PP,ExampleType,Id,Name,Value
VP,ExampleType,#,""First"",""Alpha""
VP,ExampleType,#,""Second"",""Beta""
VP,ExampleType,#,""Third"",""Gama""
";
            var options =
                new Options()
                    .ForType("ExampleType", "Id", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize text to data.");

            Assert.IsTrue(data.ContainsKey("ExampleType"), "Expected data type is not present.");

            var exampleTypeData = data["ExampleType"];
            Assert.AreEqual(3, exampleTypeData.Item1.Length, "Unexpected number of properties.");

            Assert.AreEqual("Id", exampleTypeData.Item1[0], "Unexpected property name #1.");
            Assert.AreEqual("Name", exampleTypeData.Item1[1], "Unexpected property name #2.");
            Assert.AreEqual("Value", exampleTypeData.Item1[2], "Unexpected property name #3.");

            Assert.AreEqual(3, exampleTypeData.Item2.Count(), "Unexpected number of records.");
            Assert.IsTrue(exampleTypeData.Item2.All(n => n.Length == 3), "Unexpected number of fields.");

            const string UNEXPECTED_VALUE = "Unexpected value: record {0}, {1}";

            var records = exampleTypeData.Item2.ToList();
            Assert.AreEqual("1", records[0][0], UNEXPECTED_VALUE, 0, "Id");
            Assert.AreEqual("First", records[0][1], UNEXPECTED_VALUE, 0, "Name");
            Assert.AreEqual("Alpha", records[0][2], UNEXPECTED_VALUE, 0, "Value");

            Assert.AreEqual("2", records[1][0], UNEXPECTED_VALUE, 1, "Id");
            Assert.AreEqual("Second", records[1][1], UNEXPECTED_VALUE, 1, "Name");
            Assert.AreEqual("Beta", records[1][2], UNEXPECTED_VALUE, 1, "Value");

            Assert.AreEqual("3", records[2][0], UNEXPECTED_VALUE, 2, "Id");
            Assert.AreEqual("Third", records[2][1], UNEXPECTED_VALUE, 2, "Name");
            Assert.AreEqual("Gama", records[2][2], UNEXPECTED_VALUE, 2, "Value");
        }

        /// <summary>
        /// This test proves that the typed config works for data deserialzation.
        /// </summary>
        [TestMethod]
        public void ConfigurationTypedTest()
        {
            // Assign
            var text = @"
TypedConfig,ExampleType,PropertyPrefix,PP
TypedConfig,ExampleType,ValuesPrefix,VP

PP,ExampleType,Id,Name,Value
VP,ExampleType,#,""First"",""Alpha""
VP,ExampleType,#,""Second"",""Beta""
VP,ExampleType,#,""Third"",""Gama""
";
            var options =
                new Options()
                    .ForType("ExampleType", "Id", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize text to data.");

            Assert.IsTrue(data.ContainsKey("ExampleType"), "Expected data type is not present.");

            var exampleTypeData = data["ExampleType"];
            Assert.AreEqual(3, exampleTypeData.Item1.Length, "Unexpected number of properties.");

            Assert.AreEqual("Id", exampleTypeData.Item1[0], "Unexpected property name #1.");
            Assert.AreEqual("Name", exampleTypeData.Item1[1], "Unexpected property name #2.");
            Assert.AreEqual("Value", exampleTypeData.Item1[2], "Unexpected property name #3.");

            Assert.AreEqual(3, exampleTypeData.Item2.Count(), "Unexpected number of records.");
            Assert.IsTrue(exampleTypeData.Item2.All(n => n.Length == 3), "Unexpected number of fields.");

            const string UNEXPECTED_VALUE = "Unexpected value: record {0}, {1}";

            var records = exampleTypeData.Item2.ToList();
            Assert.AreEqual("1", records[0][0], UNEXPECTED_VALUE, 0, "Id");
            Assert.AreEqual("First", records[0][1], UNEXPECTED_VALUE, 0, "Name");
            Assert.AreEqual("Alpha", records[0][2], UNEXPECTED_VALUE, 0, "Value");

            Assert.AreEqual("2", records[1][0], UNEXPECTED_VALUE, 1, "Id");
            Assert.AreEqual("Second", records[1][1], UNEXPECTED_VALUE, 1, "Name");
            Assert.AreEqual("Beta", records[1][2], UNEXPECTED_VALUE, 1, "Value");

            Assert.AreEqual("3", records[2][0], UNEXPECTED_VALUE, 2, "Id");
            Assert.AreEqual("Third", records[2][1], UNEXPECTED_VALUE, 2, "Name");
            Assert.AreEqual("Gama", records[2][2], UNEXPECTED_VALUE, 2, "Value");
        }

        /// <summary>
        /// This test proves that the global config works for data deserialzation and that 
        /// data-types that don't use the global defined prefixes are ignored.
        /// </summary>
        [TestMethod]
        public void ConfigurationGlobalOverridesDefaultTest()
        {
            // Assign
            var text = @"
GlobalConfig,PropertyPrefix,PP
GlobalConfig,ValuesPrefix,VP

PP,ExampleType,Id,Name,Value
VP,ExampleType,#,""First"",""Alpha""
VP,ExampleType,#,""Second"",""Beta""
VP,ExampleType,#,""Third"",""Gama""

Properties,IgnoredType,Id,Name,Value
Values,IgnoredType,#,""111"",""AAA""
Values,IgnoredType,#,""222"",""BBB""
";
            var options =
                new Options()
                    .ForType("ExampleType", "Id", "Name", "Value")
                    .ForType("IgnoredType", "Id", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize text to data.");

            Assert.IsTrue(data.ContainsKey("ExampleType"), "Expected data type is not present.");

            Assert.IsFalse(data.ContainsKey("IgnoredType"), "Unexpected data type is present.");
        }

        /// <summary>
        /// This test proves that the typed config works for data deserialzation and that 
        /// data-types that use the default prefixes are not ignored.
        /// </summary>
        [TestMethod]
        public void ConfigurationTypedDoesnotOverrideDefaultTest()
        {
            // Assign
            var text = @"
TypedConfig,ExampleType,PropertyPrefix,PP
TypedConfig,ExampleType,ValuesPrefix,VP

PP,ExampleType,Id,Name,Value
VP,ExampleType,#,""First"",""Alpha""
VP,ExampleType,#,""Second"",""Beta""
VP,ExampleType,#,""Third"",""Gama""

Properties,OtherType,Id,Name,Value
Values,OtherType,#,""111"",""AAA""
Values,OtherType,#,""222"",""BBB""
";
            var options =
                new Options()
                    .ForType("ExampleType", "Id", "Name", "Value")
                    .ForType("OtherType", "Id", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize text to data.");

            Assert.IsTrue(data.ContainsKey("ExampleType"), "Expected data type is not present.");

            Assert.IsTrue(data.ContainsKey("OtherType"), "Other data type is not present.");
        }
    }
}
