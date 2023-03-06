namespace Crowswood.CsvConverter.Tests.ConverterTests
{
    [TestClass]
    public class ConverterTypelessTests
    {
        [TestMethod]
        public void TypelessDeserializationSimpleTest()
        {
            TypelessDeserializationImplementation(@"
Properties,FooBar,Id,Name,Value,Thing
Values,FooBar,#,""Fred"",77,Alpha
Values,FooBar,#,""Bert"",99,Beta",
                "FooBar", new[] { "Id", "Name", "Value", "Thing", },
                new[] { "Id", "Name", "Value", "Thing", },
                new[] { "1", "Fred", "77", "Alpha", },
                new[] { "2", "Bert", "99", "Beta", });
        }

        [TestMethod]
        public void TypelessDeserializationExtraPropertiesTest()
        {
            TypelessDeserializationImplementation(@"
Properties,FooBar,Id,Name,Value,Thing,Other
Values,FooBar,#,""Fred"",77,Alpha,""Mike""
Values,FooBar,#,""Bert"",99,Beta,""Oscar""",
                "FooBar", new[] { "Id", "Name", "Value", "Thing", },
                new[] { "Id", "Name", "Value", "Thing", },
                new[] { "1", "Fred", "77", "Alpha", },
                new[] { "2", "Bert", "99", "Beta", });
        }

        [TestMethod]
        public void TypelessDeserializationExtraValuesTest()
        {
            TypelessDeserializationImplementation(@"
Properties,FooBar,Id,Name,Value,Thing
Values,FooBar,#,""Fred"",77,Alpha
Values,FooBar,#,""Bert"",99,Beta",
                "FooBar", new[] { "Id", "Name", "Value", "Thing", "Other", },
                new[] { "Id", "Name", "Value", "Thing", "Other", },
                new[] { "1", "Fred", "77", "Alpha", "", },
                new[] { "2", "Bert", "99", "Beta", "", });
        }

        [TestMethod]
        public void TypelessSerializationSimpleTest()
        {
            TypelessSerializationImplemation(
                data: new()
                {
                    ["FooBar"] =
                        (
                            new[] { "Id", "Name", "Value", "Thing" },
                            new List<string[]>
                            {
                                new[] { "1", "Fred", "77", "Alpha",},
                                new[] { "2", "Bert", "99", "Beta",},
                            }
                        ),
                },
                typeName: "FooBar", propertyNames: new[] { "Id", "Name", "Value", "Thing", },
                expectedProperties: "Properties,FooBar,Id,Name,Value,Thing",
                expectedValues: new[]
                {
                    "Values,FooBar,\"1\",\"Fred\",\"77\",\"Alpha\"",
                    "Values,FooBar,\"2\",\"Bert\",\"99\",\"Beta\"",
                });
        }

        [TestMethod]
        public void TypelessInsufficientVauesDeserializationTest()
        {
            // Arrange
            var text = @"
Properties,FooBar,Id,Name,OtherValue
Values,FooBar,1,""Fred""
";
            var options =
                new Options()
                    .ForType("FooBar", "Id", "Name", "OtherValue");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize data.");
        }

        [TestMethod]
        public void TypelessDeserializationTypelessMetadataTest()
        {
            // Arrange
            var text = @"
Metadata,FooBar,1,""Blue""
Properties,FooBar,Id,Name,Value,Thing
Values,FooBar,#,""Fred"",77,Alpha
Values,FooBar,#,""Bert"",99,Beta";
            var options =
                new Options()
                    .ForMetadata("Metadata", false, "Id", "Name")
                    .ForType("FooBar", "Id", "Name", "Value", "Thing");
            var converter = new Converter(options);

            // Act
            _ = converter.Deserialize(text).ToList();

            // Assert
            Assert.IsNotNull(converter.Metadata, "No metadata");
            Assert.IsTrue(converter.Metadata.ContainsKey("FooBar"), "No FooBar metadata.");

            Assert.AreEqual(1, converter.Metadata["FooBar"].Count, "Unexpected number of metadata items.");

            var metadata = converter.Metadata["FooBar"].First() as Dictionary<string, string>;
            Assert.IsNotNull(metadata, "Metadata for FooBar is not of the expected type.");

            Assert.AreEqual(2, metadata.Count, "Unexpected number of metadata elements for FooBar.");
            Assert.IsTrue(metadata.ContainsKey("Id"), "Metadata for FooBar does not contain element for Id.");
            Assert.AreEqual("1", metadata["Id"], "Unexpected value of metadata for FooBar element Id.");
            Assert.IsTrue(metadata.ContainsKey("Name"), "Metadata for FooBar does not contain element for Name.");
            Assert.AreEqual("Blue", metadata["Name"], "Unexpected value of metadata for FooBar element Name.");
        }

        [TestMethod]
        public void TypelessReferenceDataDeserializationTest()
        {
            // Arrange
            var text = @"
Properties,FooBar,Id,Name,ReferenceDataId
Values,FooBar,#,""Fred"",#ReferenceData(""Beta"")
Values,FooBar,#,""Bert"",#ReferenceData(""Alpha"")

Properties,ReferenceData,Id,Name
Values,ReferenceData,#,""Alpha""
Values,ReferenceData,#,""Beta""";
            var options =
                new Options()
                    .ForType("FooBar", "Id", "Name", "ReferenceDataId")
                    .ForType("ReferenceData", "Id", "Name");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize to typeless data.");
            Assert.AreEqual(2, data.Count, "Unexpected number of data types.");

            var fooBarData = data["FooBar"];
            Assert.AreEqual(2, fooBarData.Item2.Count(), "Unexpected number of items: FooBar.");

            var referenceDataData = data["ReferenceData"];
            Assert.AreEqual(2, referenceDataData.Item2.Count(), "Unexpected number of items: ReferenceData.");

            var fooBarNameIndex = fooBarData.Item1.ToList().IndexOf("Name");
            var fred = fooBarData.Item2.FirstOrDefault(item => item[fooBarNameIndex] == "Fred");
            var bert = fooBarData.Item2.FirstOrDefault(item => item[fooBarNameIndex] == "Bert");

            var referenceDataNameIndex = referenceDataData.Item1.ToList().IndexOf("Name");
            var alpha = referenceDataData.Item2.FirstOrDefault(item => item[referenceDataNameIndex] == "Alpha");
            var beta = referenceDataData.Item2.FirstOrDefault(item => item[referenceDataNameIndex] == "Beta");

            Assert.IsNotNull(fred, "Failed to find record: Fred.");
            Assert.IsNotNull(bert, "Failed to find record: Bert.");
            Assert.IsNotNull(alpha, "Failed to find record: Alpha.");
            Assert.IsNotNull(beta, "Failed to find record: Beta.");

            var fooBarReferenceDataIdIndex = data["FooBar"].Item1.ToList().IndexOf("ReferenceDataId");
            var referenceDataIdIndex = data["ReferenceData"].Item1.ToList().IndexOf("Id");

            Assert.AreEqual(alpha[referenceDataIdIndex], bert[fooBarReferenceDataIdIndex],
                "Unexpected reference value: Bert.");
            Assert.AreEqual(beta[referenceDataIdIndex], fred[fooBarReferenceDataIdIndex],
                "Unexpected reference value: Fred.");
        }

        [TestMethod]
        public void TypelessSelfReferencingDataDeserialisationTest()
        {
            // Arrange
            var text = @"
Properties,FooBar,Id,FooId,Name
Values,FooBar,#,#FooBar(""Beta""),""Alpha""
Values,FooBar,#,#FooBar(""Alpha""),""Beta""
";
            var options =
            new Options()
                .ForType("FooBar", "Id", "Name", "FooId");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize to typeless data.");
            Assert.AreEqual(1, data.Count, "Unexpected number of data types.");

            var fooBarData = data["FooBar"];
            Assert.AreEqual(2, fooBarData.Item2.Count(), "Unexpected number of items: FooBar.");

            var fooBarNameIndex =
                fooBarData.Item1
                    .ToList()
                    .IndexOf("Name");

            var alpha =
                fooBarData.Item2
                    .FirstOrDefault(item => item[fooBarNameIndex] == "Alpha");
            var beta =
                fooBarData.Item2
                    .FirstOrDefault(item => item[fooBarNameIndex] == "Beta");
            Assert.IsNotNull(alpha ?? beta, "Expected records do not exist.");

            var fooBarIdIndex =
                fooBarData.Item1
                    .ToList()
                    .IndexOf("Id");
            var fooBarFooIdIndex =
                fooBarData.Item1
                    .ToList()
                    .IndexOf("FooId");

            Assert.AreEqual(alpha?[fooBarIdIndex], beta?[fooBarFooIdIndex],
                "Failed to find reference record: alpha.");
            Assert.AreEqual(beta?[fooBarIdIndex], alpha?[fooBarFooIdIndex],
                "Failed to find reference record: beta.");
        }

        [TestMethod]
        public void TypelessReferenceDataDeserializationOptionsTest()
        {
            // Arrange
            var text = @"
Properties,FooBar,Id,Name,RefDataId,RefMeatId,RefVegId
Values,FooBar,#,""Fred"",#ReferenceData(""Beta""),#ReferenceMeat(""Steak""),#ReferenceVeg(""Beans"")
Values,FooBar,#,""Bert"",#ReferenceData(""Alpha""),#ReferenceMeat(""Chops""),#ReferenceVeg(""Peas"")

Properties,ReferenceData,Identity,FullName
Values,ReferenceData,#,""Alpha""
Values,ReferenceData,#,""Beta""

Properties,ReferenceMeat,Identity,OtherName
Values,ReferenceMeat,#,""Steak""
Values,ReferenceMeat,#,""Chops""

Properties,ReferenceVeg,Id,Name
Values,ReferenceVeg,#,""Peas""
Values,ReferenceVeg,#,""Beans""";
            var options =
                new Options()
                    .ForType("FooBar", "Id", "Name", "RefDataId", "RefMeatId", "RefVegId")
                    .ForType("ReferenceData", "Identity", "FullName")
                    .ForType("ReferenceMeat", "Identity", "OtherName")
                    .ForType("ReferenceVeg", "Id", "Name")
                    .ForReferences("Identity", "FullName")
                    .ForReferences("ReferenceMeat", "Identity", "OtherName")
                    .ForReferences("ReferenceVeg", "Id", "Name");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize to typeless data.");
            Assert.AreEqual(4, data.Count, "Unexpected number of data types.");

            const string UNEXPECTED_NUMBER_ITEMS = "Unexpected number of items: {0}.";

            var fooBarData = data["FooBar"];
            Assert.AreEqual(2, fooBarData.Item2.Count(), UNEXPECTED_NUMBER_ITEMS, "FooBar");

            var referenceDataData = data["ReferenceData"];
            Assert.AreEqual(2, referenceDataData.Item2.Count(), UNEXPECTED_NUMBER_ITEMS, "ReferenceData");

            var referenceMeatData = data["ReferenceMeat"];
            Assert.AreEqual(2, referenceMeatData.Item2.Count(), UNEXPECTED_NUMBER_ITEMS, "ReferenceMeat");

            var referenceVegData = data["ReferenceVeg"];
            Assert.AreEqual(2, referenceVegData.Item2.Count(), UNEXPECTED_NUMBER_ITEMS, "ReferenceVeg");

            var (fred, bert) = Get2Items(fooBarData, "Name", "Fred", "Bert");
            var (alpha, beta) = Get2Items(referenceDataData, "FullName", "Alpha", "Beta");
            var (steak, chops) = Get2Items(referenceMeatData, "OtherName", "Steak", "Chops");
            var (peas, beans) = Get2Items(referenceVegData, "Name", "Peas", "Beans");

            var fooBarRefDataIdIndex = getIndex(fooBarData, "RefDataId");
            var fooBarRefMeatIdIndex = getIndex(fooBarData, "RefMeatId");
            var fooBarRefVegIdIndex = getIndex(fooBarData, "RefVegId");
            var referenceDataIdIndex = getIndex(referenceDataData, "Identity");
            var referenceMeatIdIndex = getIndex(referenceMeatData, "Identity");
            var referenceVegIdIndex = getIndex(referenceVegData, "Id");

            const string UNEXPECTED_REFERENCE_VALUE = "Unexpected reference value for {0} in record {1}.";

            Assert.AreEqual(beta[referenceDataIdIndex], fred[fooBarRefDataIdIndex],
                UNEXPECTED_REFERENCE_VALUE, "ReferenceData", "Fred");
            Assert.AreEqual(steak[referenceMeatIdIndex], fred[fooBarRefMeatIdIndex],
                UNEXPECTED_REFERENCE_VALUE, "ReferenceMeat", "Fred");
            Assert.AreEqual(beans[referenceVegIdIndex], fred[fooBarRefVegIdIndex],
                UNEXPECTED_REFERENCE_VALUE, "ReferenceVeg", "Fred");

            Assert.AreEqual(alpha[referenceDataIdIndex], bert[fooBarRefDataIdIndex],
                UNEXPECTED_REFERENCE_VALUE, "ReferenceData", "Bert");
            Assert.AreEqual(chops[referenceMeatIdIndex], bert[fooBarRefMeatIdIndex],
                UNEXPECTED_REFERENCE_VALUE, "ReferenceMeat", "Bert");
            Assert.AreEqual(peas[referenceVegIdIndex], bert[fooBarRefVegIdIndex],
                UNEXPECTED_REFERENCE_VALUE, "ReferenceVeg", "Bert");

            (string[], string[]) Get2Items((string[], IEnumerable<string[]>) source, string fieldName, string item1Name, string item2Name)
            {
                var index = source.Item1.ToList().IndexOf(fieldName);
                var item1Value = source.Item2.FirstOrDefault(item => item[index] == item1Name);
                var item2Value = source.Item2.FirstOrDefault(item => item[index] == item2Name);

                Assert.IsNotNull(item1Value, "Failed to find record: {0}", item1Name);
                Assert.IsNotNull(item2Value, "Failed to find record: {0}", item2Name);

                return (item1Value, item2Value);
            }

            int getIndex((string[], IEnumerable<string[]>) source, string indexName) =>
                source.Item1.ToList().IndexOf(indexName);
        }

        #region Support routines

        private static void TypelessDeserializationImplementation(string text,
                                                                  string typeName, string[] propertyNames,
                                                                  string[] expectedNames,
                                                                  params string[][] expectedValues)
        {
            // Arrange
            var options =
                new Options()
                    .ForType(typeName, propertyNames[0], propertyNames[1..^0]);
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize to dynamic data.");
            Assert.AreEqual(1, data.Count, "Unexpected number of data types.");

            var actualNames = data[typeName].Item1;
            Assert.AreEqual(expectedNames.Length, actualNames.Length,
                "Unexpected number of {0} names.", typeName);

            for (var index = 0; index < expectedNames.Length; index++)
                Assert.AreEqual(expectedNames[index], actualNames[index],
                    "Unexpected name of {0} names {1}.", typeName, index);

            var actualValues = data[typeName].Item2;
            Assert.AreEqual(expectedValues.Length, actualValues.Count(),
                "Unexpected number of {0} values.", typeName);

            for (var index = 0; index < expectedValues.Length; index++)
            {
                var actualValuesN = actualValues.Skip(index).First();
                Assert.AreEqual(expectedValues[index].Length, actualValuesN.Length,
                    "Unexpected number of {0} {1} values.", typeName, index);
                for (var innerIndex = 0; innerIndex < expectedValues[index].Length; innerIndex++)
                    Assert.AreEqual(actualValuesN[innerIndex], expectedValues[index][innerIndex],
                        "Unexpected value of {0} {1} values {2}.", typeName, index, innerIndex);
            }
        }

        private static void TypelessSerializationImplemation(Dictionary<string, (string[], IEnumerable<string[]>)> data,
                                                             string typeName, string[] propertyNames,
                                                             string expectedProperties,
                                                             params string[] expectedValues)
        {
            // Arrange
            var options =
                new Options()
                    .ForType(typeName, propertyNames[0], propertyNames[1..^0]);
            var converter = new Converter(options);

            // Act
            var text = converter.Serialize(data);

            // Assert
            Assert.IsNotNull(text, "Failed to serialize dynamic data.");

            Assert.IsTrue(text.Contains(expectedProperties), "Serialized text contains no properties line.");
            for (var index = 0; index < expectedValues.Length; index++)
                Assert.IsTrue(text.Contains(expectedValues[index]), "Serialized text contains no values line {0}.", index);
        }

        #endregion

    }
}
