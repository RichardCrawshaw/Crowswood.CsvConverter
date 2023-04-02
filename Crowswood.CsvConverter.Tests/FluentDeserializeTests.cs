using System.Reflection;

namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class FluentDeserializeTests
    {
        private const string FAILED_DESERIALIZE = "Failed to deserialize {0}.";
        private const string UNEXPECTED_COUNT = "Unexpected number of records for {0}.";
        private const string UNEXPECTED_TYPE = "Unexpected data type.";
        private const string UNEXPECTED_VALUE = "Unexpected {0} element {1} {2}.";
        private const string UNEXPECTED_PROPERTY_NAME = "Unexpected property name in {0}.";

        [TestMethod]
        public void InitialisationTest()
        {
            // Arrange
            var text = "";
            var options = Options.None;
            var converter = new Converter(options);

            // Act
            Exception? exception = null;
            try
            {
                converter
                    .Initialise(text)
                    .Deserialize();

            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.IsNull(exception, "Exception thrown during deserialization initalisation.");
        }

        [TestMethod]
        public void TypedDataGenericTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options = Options.None;
            var converter = new Converter(options);

            // Act
            var deserializer = 
                converter
                    .Initialise(text)
                    .Data<Foo>()
                    .Deserialize();
            var fooData =
                deserializer
                    .GetData<Foo>();

            // Assert
            Assert.IsNotNull(fooData, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, fooData.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");
        }

        [TestMethod]
        public void TypedDataTypeTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options = Options.None;
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data(typeof(Foo))
                    .Deserialize();
            var data =
                deserializer
                    .GetData(typeof(Foo));

            // Assert
            Assert.IsNotNull(data, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, data.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, data.Count(item => item.GetType() != typeof(Foo)), UNEXPECTED_TYPE);

            var fooData =
                data.Cast<Foo>()
                    .ToList();

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");
        }

        [TestMethod]
        public void TypedDataNameTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options = Options.None;
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data("Foo")
                    .Deserialize();
            var data =
                deserializer
                    .GetData(typeof(Foo));

            // Assert
            Assert.IsNotNull(data, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, data.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, data.Count(item => item.GetType() != typeof(Foo)), UNEXPECTED_TYPE);

            var fooData =
                data.Cast<Foo>()
                    .ToList();

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");
        }

        [TestMethod]
        public void TypelessDataTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options = Options.None;
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data("Foo")
                    .Deserialize();
            var fooData =
                deserializer
                    .GetData("Foo");

            // Assert
            Assert.IsNotNull(fooData, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, fooData.Names.Length, UNEXPECTED_COUNT, "Property Names of Foo");
            Assert.AreEqual("Id", fooData.Names[0], UNEXPECTED_PROPERTY_NAME, "Foo");
            Assert.AreEqual("Name", fooData.Names[1], UNEXPECTED_PROPERTY_NAME, "Foo");

            Assert.AreEqual(2, fooData.Values.Count(), UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, fooData.Values.Count(values => values.Length != 2), 
                "Unexpected number of elements in one or more values.");

            var fooValues =
                fooData.Values
                    .ToArray();

            Assert.AreEqual("1",    fooValues[0][0], UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooValues[0][1], UNEXPECTED_VALUE, "Foo", 0, "Name");
            Assert.AreEqual("2",    fooValues[1][0], UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooValues[1][1], UNEXPECTED_VALUE, "Foo", 1, "Name");
        }

        [TestMethod]
        public void TypedDataGeneric_TypedMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options =
                new Options()
                    .ForMetadata<SomeMetadata>("SomeMetadata", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data<Foo>()
                    .Deserialize();
            var fooData = 
                deserializer
                    .GetData<Foo, SomeMetadata>(out var fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */

            Assert.IsNotNull(fooData, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, fooData.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, typeof(SomeMetadata).Name);

            Assert.AreEqual(1, fooSomeMetadata.Count, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual("East", fooSomeMetadata[0].Name, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadata[0].Value, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void TypedDataType_TypedMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options =
                new Options()
                    .ForMetadata<SomeMetadata>("SomeMetadata", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data(typeof(Foo))
                    .Deserialize();
            var data = 
                deserializer
                    .GetData<SomeMetadata>(typeof(Foo), out var fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */
            
            Assert.IsNotNull(data, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, data.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, data.Count(item => item.GetType() != typeof(Foo)), UNEXPECTED_TYPE);

            var fooData =
                data.Cast<Foo>()
                    .ToList();

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, typeof(SomeMetadata).Name);

            Assert.AreEqual(1, fooSomeMetadata.Count, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual("East", fooSomeMetadata[0].Name, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadata[0].Value, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void TypedDataName_TypedMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options =
                new Options()
                    .ForMetadata<SomeMetadata>("SomeMetadata", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data("Foo")
                    .Deserialize();
            var data = 
                deserializer
                    .GetData<SomeMetadata>(typeof(Foo), out var fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */

            Assert.IsNotNull(data, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, data.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, data.Count(item => item.GetType() != typeof(Foo)), UNEXPECTED_TYPE);

            var fooData =
                data.Cast<Foo>()
                    .ToList();

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, typeof(SomeMetadata).Name);

            Assert.AreEqual(1, fooSomeMetadata.Count, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual("East", fooSomeMetadata[0].Name, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadata[0].Value, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void TypelessData_TypedMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options = 
                new Options()
                    .ForMetadata<SomeMetadata>("SomeMetadata", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data("Foo")
                    .Deserialize();
            var fooData = 
                deserializer
                    .GetData<SomeMetadata>("Foo", out List<SomeMetadata> fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */

            Assert.IsNotNull(fooData, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, fooData.Names.Length, "Unexpected number of property names.");
            Assert.AreEqual("Id", fooData.Names[0], UNEXPECTED_PROPERTY_NAME, "Foo");
            Assert.AreEqual("Name", fooData.Names[1], UNEXPECTED_PROPERTY_NAME, "Foo");

            Assert.AreEqual(2, fooData.Values.Count(), UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, fooData.Values.Count(values => values.Length != 2), 
                "Unexpected number of elements in one or more values.");

            var fooValues =
                fooData.Values
                    .ToArray();

            Assert.AreEqual("1",    fooValues[0][0], UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooValues[0][1], UNEXPECTED_VALUE, "Foo", 0, "Name");
            Assert.AreEqual("2",    fooValues[1][0], UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooValues[1][1], UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, typeof(SomeMetadata).Name);

            Assert.AreEqual(1, fooSomeMetadata.Count, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual("East", fooSomeMetadata[0].Name, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadata[0].Value, UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void TypedDataGeneric_TypelessMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options =
                new Options()
                    .ForMetadata("SomeMetadata", allowNulls: true, "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data<Foo>()
                    .Deserialize();
            var fooData = 
                deserializer.GetData<Foo>(
                    metadataTypeName: "SomeMetadata", 
                    out var fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */

            Assert.IsNotNull(fooData, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, fooData.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, "SomeMetadata");

            Assert.AreEqual(2, fooSomeMetadata.Names.Length, UNEXPECTED_COUNT, "Property Names of SomeMetadata for Foo");
            Assert.AreEqual("Name", fooSomeMetadata.Names[0], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");
            Assert.AreEqual("Value", fooSomeMetadata.Names[1], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");

            var fooSomeMetadataValues =
                fooSomeMetadata.Values
                    .ToArray();

            Assert.AreEqual(1, fooSomeMetadataValues.Length, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual(0, fooSomeMetadataValues.Count(values => values.Length != 2),
                "Unexpected number of elements in one or more values.");

            Assert.AreEqual("East",    fooSomeMetadataValues[0][0], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadataValues[0][1], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void TypedDataType_TypelessMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options =
                new Options()
                    .ForMetadata("SomeMetadata", allowNulls: true, "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data(typeof(Foo))
                    .Deserialize();
            var data =
                deserializer.GetData(
                    typeof(Foo),
                    metadataTypeName: "SomeMetadata",
                    out var fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */

            Assert.IsNotNull(data, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, data.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, data.Count(item => item.GetType() != typeof(Foo)), UNEXPECTED_TYPE);

            var fooData =
                data.Cast<Foo>()
                    .ToList();

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, "SomeMetadata");

            Assert.AreEqual(2, fooSomeMetadata.Names.Length, UNEXPECTED_COUNT, "Property Names of SomeMetadata for Foo");
            Assert.AreEqual("Name", fooSomeMetadata.Names[0], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");
            Assert.AreEqual("Value", fooSomeMetadata.Names[1], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");

            var fooSomeMetadataValues =
                fooSomeMetadata.Values
                    .ToArray();

            Assert.AreEqual(1, fooSomeMetadataValues.Length, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual(0, fooSomeMetadataValues.Count(values => values.Length != 2),
                "Unexpected number of elements in one or more values.");

            Assert.AreEqual("East", fooSomeMetadataValues[0][0], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadataValues[0][1], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void TypedDataName_TypelessMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options =
                new Options()
                    .ForMetadata("SomeMetadata", allowNulls: true, "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data("Foo")
                    .Deserialize();
            var data =
                deserializer
                    .GetData(typeof(Foo), "SomeMetadata", out var fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */

            Assert.IsNotNull(data, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, data.Count, UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, data.Count(item => item.GetType() != typeof(Foo)), UNEXPECTED_TYPE);

            var fooData =
                data.Cast<Foo>()
                    .ToList();

            Assert.AreEqual(1, fooData[0].Id, UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooData[0].Name, UNEXPECTED_VALUE, "Foo", 0, "Name");

            Assert.AreEqual(2, fooData[1].Id, UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooData[1].Name, UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, "SomeMetadata");

            Assert.AreEqual(2, fooSomeMetadata.Names.Length, UNEXPECTED_COUNT, "Property Names of SomeMetadata for Foo");
            Assert.AreEqual("Name", fooSomeMetadata.Names[0], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");
            Assert.AreEqual("Value", fooSomeMetadata.Names[1], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");

            var fooSomeMetadataValues =
                fooSomeMetadata.Values
                    .ToArray();

            Assert.AreEqual(1, fooSomeMetadataValues.Length, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual(0, fooSomeMetadataValues.Count(values => values.Length != 2),
                "Unexpected number of elements in one or more values.");

            Assert.AreEqual("East", fooSomeMetadataValues[0][0], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadataValues[0][1], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void TypelessData_TypelessMetadata_Test()
        {
            // Arrange
            var text = @"
SomeMetadata,Foo,""East"",""Tuesday""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
Values,Foo,2,""Bert""";
            var options =
                new Options()
                    .ForMetadata("SomeMetadata", allowNulls: true, "Name", "Value");
            var converter = new Converter(options);

            // Act
            var deserializer =
                converter
                    .Initialise(text)
                    .Data("Foo")
                    .Deserialize();
            var fooData = 
                deserializer
                    .GetData("Foo", "SomeMetadata", out var fooSomeMetadata);

            // Assert

            /* Confirm that we still get the basic data when there is metadata as well. */

            Assert.IsNotNull(fooData, FAILED_DESERIALIZE, "Foo");

            Assert.AreEqual(2, fooData.Names.Length, "Unexpected number of property names.");
            Assert.AreEqual("Id", fooData.Names[0], UNEXPECTED_PROPERTY_NAME, "Foo");
            Assert.AreEqual("Name", fooData.Names[1], UNEXPECTED_PROPERTY_NAME, "Foo");

            Assert.AreEqual(2, fooData.Values.Count(), UNEXPECTED_COUNT, "Foo");

            Assert.AreEqual(0, fooData.Values.Count(values => values.Length != 2),
                "Unexpected number of elements in one or more values.");

            var fooValues =
                fooData.Values
                    .ToArray();

            Assert.AreEqual("1", fooValues[0][0], UNEXPECTED_VALUE, "Foo", 0, "Id");
            Assert.AreEqual("Fred", fooValues[0][1], UNEXPECTED_VALUE, "Foo", 0, "Name");
            Assert.AreEqual("2", fooValues[1][0], UNEXPECTED_VALUE, "Foo", 1, "Id");
            Assert.AreEqual("Bert", fooValues[1][1], UNEXPECTED_VALUE, "Foo", 1, "Name");

            /* Now confirm the metadata. */

            Assert.IsNotNull(fooSomeMetadata, FAILED_DESERIALIZE, "SomeMetadata");

            Assert.AreEqual(2, fooSomeMetadata.Names.Length, UNEXPECTED_COUNT, "Property Names of SomeMetadata for Foo");
            Assert.AreEqual("Name", fooSomeMetadata.Names[0], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");
            Assert.AreEqual("Value", fooSomeMetadata.Names[1], UNEXPECTED_PROPERTY_NAME, "SomeMetadata for Foo");

            var fooSomeMetadataValues =
                fooSomeMetadata.Values
                    .ToArray();

            Assert.AreEqual(1, fooSomeMetadataValues.Length, UNEXPECTED_COUNT, "SomeMetadata for Foo");

            Assert.AreEqual(0, fooSomeMetadataValues.Count(values => values.Length != 2),
                "Unexpected number of elements in one or more values.");

            Assert.AreEqual("East", fooSomeMetadataValues[0][0], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Name");
            Assert.AreEqual("Tuesday", fooSomeMetadataValues[0][1], UNEXPECTED_VALUE, "SomeMetadata for Foo", 0, "Value");
        }

        [TestMethod]
        public void RetrieveTypes_DefaultPrefixes_Test()
        {
            // Arrange
            var text = @"
Properties,Foo1,Id,Name
Values,Foo1,1,""Fred""

Properties,Bar1,Id,Value
Values,Bar1,2,""Tuesday""

Properties,Baz1,Id,Number
Values,Baz1,3,77.3";
            var options = Options.None;
            var converter = new Converter(options);
            var deserializer =
                converter
                    .Initialise(text);

            // Act
            var types1 = deserializer.GetTypes();
            var types2 = deserializer.GetTypes(typeof(Foo1), typeof(Bar1), typeof(Baz1));
            var types3 = deserializer.GetTypes(Assembly.GetExecutingAssembly());

            // Assert
            Assert.IsNotNull(types1, "Failed to retrieve types from default.");
            Assert.IsNotNull(types2, "Failed to retrieve types from type list.");
            Assert.IsNotNull(types3, "Failed to retrieve types from assembly.");

            Assert.AreEqual(3, types1.Count, "Unexpected number of types from default.");
            Assert.AreEqual(3, types2.Count, "Unexpected number of types from type list.");
            Assert.AreEqual(3, types3.Count, "Unexpected number of types from assembly.");

            const string MISSING_TYPE = "Types from {1} does not contain {2}.";

            Assert.IsTrue(types1.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "default");
            Assert.IsTrue(types1.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "default");
            Assert.IsTrue(types1.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "default");

            Assert.IsTrue(types2.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "type list");
            Assert.IsTrue(types2.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "type list");
            Assert.IsTrue(types2.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "type list");

            Assert.IsTrue(types3.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "assembly");
            Assert.IsTrue(types3.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "assembly");
            Assert.IsTrue(types3.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "assembly");
        }

        [TestMethod]
        public void RetrieveTypes_OptionsPrefixes_Test()
        {
            // Arrange
            var text = @"
PZ,Foo1,Id,Name
VX,Foo1,1,""Fred""

PZ,Bar1,Id,Value
VX,Bar1,2,""Tuesday""

PZ,Baz1,Id,Number
VX,Baz1,3,77.3";
            var options =
                new Options()
                    .SetPrefixes("PZ", "VX");
            var converter = new Converter(options);
            var deserializer =
                converter
                    .Initialise(text);

            // Act
            var types1 = deserializer.GetTypes();
            var types2 = deserializer.GetTypes(typeof(Foo1), typeof(Bar1), typeof(Baz1));
            var types3 = deserializer.GetTypes(Assembly.GetExecutingAssembly());

            // Assert
            Assert.IsNotNull(types1, "Failed to retrieve types from default.");
            Assert.IsNotNull(types2, "Failed to retrieve types from type list.");
            Assert.IsNotNull(types3, "Failed to retrieve types from assembly.");

            Assert.AreEqual(3, types1.Count, "Unexpected number of types from default.");
            Assert.AreEqual(3, types2.Count, "Unexpected number of types from type list.");
            Assert.AreEqual(3, types3.Count, "Unexpected number of types from assembly.");

            const string MISSING_TYPE = "Types from {1} does not contain {2}.";

            Assert.IsTrue(types1.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "default");
            Assert.IsTrue(types1.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "default");
            Assert.IsTrue(types1.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "default");

            Assert.IsTrue(types2.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "type list");
            Assert.IsTrue(types2.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "type list");
            Assert.IsTrue(types2.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "type list");

            Assert.IsTrue(types3.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "assembly");
            Assert.IsTrue(types3.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "assembly");
            Assert.IsTrue(types3.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "assembly");
        }

        [TestMethod]
        public void RetrieveTypes_ConfigPrefixes_Test()
        {
            // Arrange
            var text = @"
GlobalConfig,PropertyPrefix,PZ
GlobalConfig,ValuePrefix,VX

TypedConfig,Baz1,PropertyPrefix,ZP
TypedConfig,Baz1,ValuePrefix,XV

PZ,Foo1,Id,Name
VX,Foo1,1,""Fred""

PZ,Bar1,Id,Value
VX,Bar1,2,""Tuesday""

ZP,Baz1,Id,Number
XV,Baz1,3,77.3";
            var options = Options.None;
            var converter = new Converter(options);
            var deserializer =
                converter
                    .Initialise(text);

            // Act
            var types1 = deserializer.GetTypes();
            var types2 = deserializer.GetTypes(typeof(Foo1), typeof(Bar1), typeof(Baz1));
            var types3 = deserializer.GetTypes(Assembly.GetExecutingAssembly());

            // Assert
            Assert.IsNotNull(types1, "Failed to retrieve types from default.");
            Assert.IsNotNull(types2, "Failed to retrieve types from type list.");
            Assert.IsNotNull(types3, "Failed to retrieve types from assembly.");

            Assert.AreEqual(3, types1.Count, "Unexpected number of types from default.");
            Assert.AreEqual(3, types2.Count, "Unexpected number of types from type list.");
            Assert.AreEqual(3, types3.Count, "Unexpected number of types from assembly.");

            const string MISSING_TYPE = "Types from {1} does not contain {2}.";

            Assert.IsTrue(types1.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "default");
            Assert.IsTrue(types1.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "default");
            Assert.IsTrue(types1.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "default");

            Assert.IsTrue(types2.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "type list");
            Assert.IsTrue(types2.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "type list");
            Assert.IsTrue(types2.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "type list");

            Assert.IsTrue(types3.Contains(typeof(Foo1)), MISSING_TYPE, typeof(Foo1).Name, "assembly");
            Assert.IsTrue(types3.Contains(typeof(Bar1)), MISSING_TYPE, typeof(Bar1).Name, "assembly");
            Assert.IsTrue(types3.Contains(typeof(Baz1)), MISSING_TYPE, typeof(Baz1).Name, "assembly");
        }

        [TestMethod]
        public void Retrieve_TypeNames()
        {
            // Arrange
            var text = @"
Properties,Foo1,Id,Name
Values,Foo1,1,""Fred""

Properties,Bar1,Id,Value
Values,Bar1,2,""Tuesday""

Properties,Baz1,Id,Number
Values,Baz1,3,77.3";
            var options = Options.None;
            var converter = new Converter(options);
            var deserializer =
                converter
                    .Initialise(text);

            // Act
            var typeNames = deserializer.GetTypeNames();

            // Assert

            Assert.IsNotNull(typeNames, "Failed to retrieve type names.");

            Assert.AreEqual(3, typeNames.Count, "Unexpected number of type names.");

            const string MISSING_TYPE = "Types does not contain {2}.";

            Assert.IsTrue(typeNames.Contains("Foo1"), MISSING_TYPE, "Foo1");
            Assert.IsTrue(typeNames.Contains("Bar1"), MISSING_TYPE, "Bar1");
            Assert.IsTrue(typeNames.Contains("Baz1"), MISSING_TYPE, "Baz1");
        }

        #region Model

        private class Foo
        {
            public int Id { get; set; }

            public string? Name { get; set; }
        }

        private class Foo1 : Foo { }

        private class Bar1
        {
            public int Id { get; set; }
            public string? Value { get; set; }
        }

        private class Baz1
        {
            public int Id { get; set; }
            public float Number { get; set; }
        }

        private class SomeMetadata
        {
            public string? Name { get; set; }

            public string? Value { get; set; }
        }


        #endregion
    }
}
