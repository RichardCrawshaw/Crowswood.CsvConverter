using System.ComponentModel;
using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class ConverterTests
    {
        [TestMethod]
        public void ConstructorTest()
        {
            // Act
            var converter = new Converter(Options.None);

            // Assert
            Assert.IsNotNull(converter, "Failed to create CSV Converter.");
        }

        [TestMethod]
        public void DeserializeJunkTest()
        {
            // Arrange
            var converter = new Converter(Options.None);

            // Act
            var data = converter.Deserialize<object>("some text");

            // Assert
            Assert.AreEqual(0, data.Count(), "Unexpected number of objects.");
        }

        [TestMethod]
        public void DeserializeCommentsTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Fred""
# This is a hash comment
! This is a bang comment
; This is a semi-colon comment
// This is a double-slash comment
-- This is a SQL comment
    # This is a comment with leading blank space.";
            var options =
                new Options()
                    .ForType<Foo>();
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.AreEqual(1, data.Count(), "Unexpected number of Foo objects.");
        }

        [TestMethod]
        public void DeserializeEmbeddedCommaInTextTest()
        {
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Name, with embedded comma""";
            var options =
                new Options()
                    .ForType<Foo>();
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.AreEqual(1, data.Count(), "Unexpected number of Foo objects.");

            Assert.AreEqual(1, data.First().Id, "Unexpected value for Id of Foo 0.");
            Assert.AreEqual("Name, with embedded comma", data.First().Name, "Unexpected value for Name of Foo 0.");
        }

        [TestMethod]
        public void SimpleDeserializeTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name,TestEnum
Values,Foo,1,""Fred"",TestEnum.Data";
            var options =
                new Options()
                    .ForType<Foo>();
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.AreEqual(1, data.Count(), "Unexpected number of Foo objects.");

            Assert.AreEqual(1, data.First().Id, "Incorrect Id.");
            Assert.AreEqual("Fred", data.First().Name, "Incorrect Name.");
            Assert.AreEqual(TestEnum.Data, data.First().TestEnum, "Incorrect TestEnum.");
        }

        [TestMethod]
        public void SimpleSerializeTest()
        {
            // Arrange
            var data =
                new List<Foo>()
                {
                    new Foo { Id = 1, Name = "Foo 1", TestEnum = TestEnum.Value },
                };

            var options =
                new Options()
                    .ForType<Foo>();
            var converter = new Converter(options);

            // Act
            var text = converter.Serialize(data);

            // Assert
            Assert.IsNotNull(text, "Failed to serialize List of Foo.");
            Assert.IsTrue(text.Contains("Properties,Foo,Id,Name,TestEnum"), "Serialized data does not contain Foo properties line.");
            Assert.IsTrue(text.Contains("Values,Foo,1,\"Foo 1\",TestEnum.Value"), "Serialized data does not contain Foo values line 0.");
        }

        [TestMethod]
        public void DerivedDeserializeTest()
        {
            // Arrange
            var text = @"
Properties,Bar,Id,Name,Flag,Value,Number
Values,Bar,1,""Fred"",true,99.9,88.88
Values,Bar,2,""Bert"",false,77.5,-321.4";
            var options =
                new Options()
                    .ForType<Bar>();
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Bar>(text);

            // Assert
            Assert.AreEqual(2, data.Count(), "Unexpected number of Bar objects.");

            Assert.AreEqual(1, data.First().Id, "Incorrect Id for 0.");
            Assert.AreEqual("Fred", data.First().Name, "Incorrect Name for 0.");
            Assert.AreEqual(TestEnum.None, data.First().TestEnum, "Incorrect TestEnum for 0.");
            Assert.AreEqual(true, data.First().Flag, "Incorrect Flag for 0.");
            Assert.AreEqual(99.9d, data.First().Value, "Incorrect Value for 0.");
            Assert.AreEqual(88.88m, data.First().Number, "Incorrect Number for 0.");

            Assert.AreEqual(2, data.Skip(1).First().Id, "Incorrect Id for 1.");
            Assert.AreEqual("Bert", data.Skip(1).First().Name, "Incorrect Name for 1.");
            Assert.AreEqual(TestEnum.None, data.First().TestEnum, "Incorrect TestEnum for 1.");
            Assert.AreEqual(false, data.Skip(1).First().Flag, "Incorrect Flag for 1.");
            Assert.AreEqual(77.5d, data.Skip(1).First().Value, "Incorrect Value for 1.");
            Assert.AreEqual(-321.4m, data.Skip(1).First().Number, "Incorrect Number for 1.");
        }

        [TestMethod]
        public void MultipleDeserializeTest()
        {
            // Arrange
            var text = @"
Properties,Bar,Id,Name,Flag,Value,Number
Values,Bar,1,""Fred"",true,99.9,88.88
Values,Bar,2,""Bert"",false,77.5,-321.4

Properties,Baz,Id,Name,Code,Description
Values,Baz,99,""Ninty-nine"",""nn"",""This value is 99""
Values,Baz,108,""One-zero-eight"",""oze"",""Value over a hundred""";
            var options =
                new Options()
                    .ForType<Bar>()
                    .ForType<Baz>();
            var converter = new Converter(options);

            // Act
            var fooData = converter.Deserialize<Foo>(text);
            var barData = fooData.Where(foo => foo is Bar);
            var bazData = fooData.Where(foo => foo is Baz);

            var barData2 = converter.Deserialize<Bar>(text);
            var bazData2 = converter.Deserialize<Baz>(text);

            // Assert
            Assert.AreEqual(4, fooData.Count(), "Unexpected number of Foo objects.");
            Assert.AreEqual(2, barData.Count(), "Unexpected number of Bar objects.");
            Assert.AreEqual(2, bazData.Count(), "Unexpected number of Baz objects.");

            Assert.AreEqual(2, barData2.Count(), "Unexpected number of Bar 2 objects.");
            Assert.AreEqual(2, bazData2.Count(), "Unexpected number of Baz 2 objects.");

            Assert.AreEqual(99, bazData2.First().Id, "Incorrect Id for Baz 0.");
            Assert.AreEqual("Ninty-nine", bazData2.First().Name, "Incorrect Name for Baz 0.");
            Assert.AreEqual("nn", bazData2.First().Code, "Incorrect Code for Baz 0.");
            Assert.AreEqual("This value is 99", bazData2.First().Description, "Incorrect Description for Baz 0.");

            Assert.AreEqual(108, bazData2.Skip(1).First().Id, "Incorrect Id for Baz 1.");
            Assert.AreEqual("One-zero-eight", bazData2.Skip(1).First().Name, "Incorrect Name for Baz 1.");
            Assert.AreEqual("oze", bazData2.Skip(1).First().Code, "Incorrect Code for Baz 1.");
            Assert.AreEqual("Value over a hundred", bazData2.Skip(1).First().Description, "Incorrect Description for Baz 1.");
        }

        [TestMethod]
        public void MultipleSerializeTest()
        {
            // Arrange
            var data =
                new List<Foo>()
                {
                    new Foo() { Id = 1, Name = "Foo 1", },
                    new Bar() { Id = 2, Name = "Bar 2", Flag = true, Value = 888.08d, Number = 9966.3m, },
                    new Baz() { Id = 3, Name = "Baz 3", Code = "iqzy", Description = "This is some text", },
                };

            var options =
                new Options()
                    .ForType<Foo>()
                    .ForType<Bar>()
                    .ForType<Baz>();
            var converter = new Converter(options);

            // Act
            var text = converter.Serialize(data);

            // Assert
            Assert.IsNotNull(text, "Failed to serialize List of Foo.");
            Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger.LogMessage(text);

            const string FOO_PROPERTY_LINE = "Properties,Foo,Id,Name,TestEnum";
            const string BAR_PROPERTY_LINE = "Properties,Bar,Id,Name,TestEnum,Flag,Value,Number";
            const string BAZ_PROPERTY_LINE = "Properties,Baz,Id,Name,TestEnum,Code,Description";

            Assert.IsTrue(text.Contains(FOO_PROPERTY_LINE), 
                "Serialized data does not contain Foo properties line.");
            Assert.AreEqual(1, 
                text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    .Count(n => n == FOO_PROPERTY_LINE), 
                "Unexpected number of Foo properties lines.");
            Assert.IsTrue(text.Contains("Values,Foo,1,\"Foo 1\",TestEnum.None"), 
                "Serialized data does not contain Foo values line 0.");

            Assert.IsTrue(text.Contains(BAR_PROPERTY_LINE), 
                "Serialized data does not contain Bar properties line.");
            Assert.AreEqual(1, 
                text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    .Count(n => n == BAR_PROPERTY_LINE), 
                "Unexpected number of Bar properties lines.");
            Assert.IsTrue(text.Contains("Values,Bar,2,\"Bar 2\",TestEnum.None,True,888.08,9966.3"),
                "Serialized data does not contain Bar values line 0.");

            Assert.IsTrue(text.Contains(BAZ_PROPERTY_LINE),
                "Serialized data does not contain Baz properties line.");
            Assert.AreEqual(1, 
                text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)
                    .Count(n => n == BAZ_PROPERTY_LINE), 
                "Unexpected number of Baz properties lines.");
            Assert.IsTrue(text.Contains("Values,Baz,3,\"Baz 3\",TestEnum.None,\"iqzy\",\"This is some text\""), 
                "Serialized data does not contain Baz values line 0.");
        }

        [TestMethod]
        public void MemberDeserializeTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Identity,FullName,EnumTest
Values,Foo,1,""Fred"",TestEnum.Entity";
            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMember<Foo, int>(foo => foo.Id, "Identity")
                    .ForMember<Foo, string?>(foo => foo.Name, "FullName")
                    .ForMember<Foo, TestEnum>(foo => foo.TestEnum, "EnumTest");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.AreEqual(1, data.Count(), "Unexpected number of Foo objects.");

            Assert.AreEqual(1, data.First().Id, "Incorrect Id.");
            Assert.AreEqual("Fred", data.First().Name, "Incorrect Name.");
            Assert.AreEqual(TestEnum.Entity, data.First().TestEnum, "Incorrect TestEnum.");
        }

        [TestMethod]
        public void MemberSerializeTest()
        {
            // Arrange
            var data =
                new List<Foo>
                {
                    new Foo{ Id = 1, Name = "Drawing", TestEnum = TestEnum.Entity, },
                };
            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMember<Foo, int>(foo => foo.Id, "Identity")
                    .ForMember<Foo, string?>(foo => foo.Name, "FullName")
                    .ForMember<Foo, TestEnum>(foo => foo.TestEnum, "EnumTest");
            var converter = new Converter(options);

            // Act
            var text = converter.Serialize<Foo>(data);

            // Assert
            Assert.IsNotNull(text, "Failed to serialize data.");

            Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger.LogMessage(text);

            Assert.IsTrue(text.Contains("Properties,Foo,Identity,FullName,EnumTest"), 
                "Foo properties line not present.");
            Assert.IsTrue(text.Contains("Values,Foo,1,\"Drawing\",TestEnum.Entity"), 
                "Foo values line 0 not present.");
        }

        [TestMethod]
        public void AttributeDeserializeTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name,TestEnum
Values,Foo,1,""Picture"",TestEnum.Data";
            var converter = new Converter(Options.None);

            // Act
            var data = converter.Deserialize<AttrFoo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize into objects using attributes.");

            Assert.AreEqual(1, data.Count(), "Incorrect number of data items using attributes.");

            Assert.AreEqual(1, data.First().Identity, "Incorrect value for Identity of object 1.");
            Assert.AreEqual("Picture", data.First().FullName, "Incorrect value for FullName of object 1.");
            Assert.AreEqual(TestEnum.Data, data.First().TestEnumValue, "Incorrect value for TestEnumValue of object 1.");
        }

        [TestMethod]
        public void AttributeSerializeTest()
        {
            // Arrange
            var data =
                new List<AttrFoo>
                {
                    new AttrFoo { Identity = 1, FullName = "Picture", TestEnumValue = TestEnum.Data, },
                };

            var converter = new Converter(Options.None);

            // Act
            var text = converter.Serialize(data);

            // Assert
            Assert.IsNotNull(text, "Converter generated null.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Converter generated nothing.");

            Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger.LogMessage(text);

            Assert.IsTrue(text.Contains("Properties,Foo,Id,Name,TestEnum"), "No properties line generated for Foo.");
            Assert.IsTrue(text.Contains("Values,Foo,1,\"Picture\",TestEnum.Data"), "No values line 0 generated for Foo.");
        }

        [TestMethod]
        public void TypedInsufficientValuesDeserializationTest()
        {
            // Assign
            var text = @"
Properties,Foo,Id,Name,TestEnum
Values,Foo,1,""Fred""
";
            var converter = new Converter(Options.None);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize data.");
        }

        [TestMethod]
        public void TypelessInsufficientVauesDeserializationTest()
        {
            // Assign
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
        public void MetadataDeserializationTest()
        {
            // Arrange
            var text = @"
Bar,Foo,""Simple metadata"",77
Baz,Foo,""Attribute metadata"",99
Properties,Foo,Id,Name,TestEnum
Values,Foo,1,""Fred"",TestEnum.Data";
            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMetadata<Metadata>("Bar", "Name", "Number")
                    .ForMetadata<MetadataAttribute>("Baz", "Name", "Value");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsTrue(converter.Metadata.Any(), "No metadata available via converter.");
            Assert.IsTrue(converter.Metadata.ContainsKey(typeof(Foo).Name), "The metadata for Foo not available via converter.");
            Assert.IsTrue(converter.Metadata[typeof(Foo).Name].Any(), "Foo has an empty list of metadata.");
            Assert.IsTrue(converter.Metadata[typeof(Foo).Name].Any(md => md is Metadata), 
                "Foo does not have the metadata of type Metadata.");
            Assert.IsTrue(
                converter.Metadata[typeof(Foo).Name]
                    .NotNull<object, Metadata>()
                    .First() is 
                { 
                    Name: "Simple metadata", 
                    Number: 77, 
                }, "Foo does not have the expected Metadata values.");

            var attributes = TypeDescriptor.GetAttributes(typeof(Foo));

            Assert.IsTrue(attributes.OfType<MetadataAttribute>().Any(), 
                "MetadataAttribute not available via Type.");
            Assert.IsTrue(
                attributes.OfType<MetadataAttribute>().First() is
                {
                    Name: "Attribute metadata",
                    Value: 99,
                }, "Foo does not have the expected MetadataAttribute values.");
        }

        [TestMethod]
        public void OptionsValidationTest()
        {
            // Arrange
            var vanillaOptions = new Options();

            var optionsWithSamePropertyAndValuesPrefixes =
                new Options()
                    .SetPrefixes("bad", "bad");
            var optionsWithSameMetadataPrefixAsProperty =
                new Options()
                    .ForMetadata<Metadata>(vanillaOptions.PropertyPrefix, "Name");
            var optionsWithSameMetadataPrefixAsValues =
                new Options()
                    .ForMetadata<Metadata>(vanillaOptions.ValuesPrefix, "Name");
            var optionsWithMetadataWithInvalidNames =
                new Options()
                    .ForMetadata<Metadata>("Metadata", "Rubbish Property Name");

            // Assert
            Assert.ThrowsException<ArgumentException>(
                () => new Converter(optionsWithSamePropertyAndValuesPrefixes),
                "Failed to reject options with the same property and values prefixes.");
            Assert.ThrowsException<ArgumentException>(
                () => new Converter(optionsWithSameMetadataPrefixAsProperty),
                "Failed to reject options with metadata with same prefix as property prefix.");
            Assert.ThrowsException<ArgumentException>(
                () => new Converter(optionsWithSameMetadataPrefixAsValues),
                "Failed to reject options with metadata with same prefix as values prefix.");
            Assert.ThrowsException<ArgumentException>(
                () => new Converter(optionsWithMetadataWithInvalidNames),
                "Failed to reject options with metadata that defines invalid property names.");
        }

        [TestMethod]
        public void MetadataDictionaryNotNullDeserializeTest()
        {
            // Arrange
            var text = @"
Metadata,Foo,""Bert"",,""""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""";

            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMetadata("Metadata", false, "Name", "Value1", "Value2");

            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsTrue(converter.Metadata.Any(), "No meta-data.");
            Assert.IsTrue(converter.Metadata.ContainsKey(typeof(Foo).Name), "No meta-data for Foo.");
            Assert.IsTrue(converter.Metadata[typeof(Foo).Name].Any(), "Empty meta-data for Foo.");

            var metadata =
                converter.Metadata[typeof(Foo).Name]
                    .NotNull<object, Dictionary<string, string?>>()
                    .FirstOrDefault();
            Assert.IsNotNull(metadata, "No dictionary meta-data for Foo.");
            Assert.IsInstanceOfType(metadata, typeof(Dictionary<string, string?>),
                "The meta-data is not the expected type.");

            Assert.IsNotNull(metadata,
                "No dictionary meta-data for Foo.");
            Assert.IsTrue(metadata.ContainsKey("Name"),
                "Dictionary meta-data for Foo does not contain 'Name'.");
            Assert.IsTrue(metadata.ContainsKey("Value1"),
                "Dictionary meta-data for Foo does not contain 'Value1'.");
            Assert.IsTrue(metadata.ContainsKey("Value2"),
                "Dictionary meta-data for Foo does not contain 'Value2'.");
            Assert.AreEqual("Bert", metadata["Name"],
                "Dictionary meta-data for Foo has unexpected value for 'Name'.");
            Assert.AreEqual("", metadata["Value1"],
                "Dictionary meta-data for Foo has unexpected value for 'Value1'.");
            Assert.AreEqual("", metadata["Value2"],
                "Dictionary meta-data for Foo has unexpected value for 'Value2'.");
        }

        [TestMethod]
        public void MetadataDictionaryNullableDeserializeTest()
        {
            // Arrange
            var text = @"
Metadata,Foo,""Bert"",,""""
Properties,Foo,Id,Name
Values,Foo,1,""Fred""";

            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMetadata("Metadata", true, "Name", "Value1", "Value2");

            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsTrue(converter.Metadata.Any(), "No meta-data.");
            Assert.IsTrue(converter.Metadata.ContainsKey(typeof(Foo).Name), "No meta-data for Foo.");
            Assert.IsTrue(converter.Metadata[typeof(Foo).Name].Any(), "Empty meta-data for Foo.");

            var metadata =
                converter.Metadata[typeof(Foo).Name]
                    .NotNull<object, Dictionary<string, string?>>()
                    .FirstOrDefault();
            Assert.IsNotNull(metadata, "No dictionary meta-data for Foo.");
            Assert.IsInstanceOfType(metadata, typeof(Dictionary<string, string?>), 
                "The meta-data is not the expected type.");

            Assert.IsNotNull(metadata,
                "No dictionary meta-data for Foo.");
            Assert.IsTrue(metadata.ContainsKey("Name"),
                "Dictionary meta-data for Foo does not contain 'Name'.");
            Assert.IsTrue(metadata.ContainsKey("Value1"),
                "Dictionary meta-data for Foo does not contain 'Value1'.");
            Assert.IsTrue(metadata.ContainsKey("Value2"),
                "Dictionary meta-data for Foo does not contain 'Value2'.");
            Assert.AreEqual("Bert", metadata["Name"],
                "Dictionary meta-data for Foo has unexpected value for 'Name'.");
            Assert.AreEqual(null, metadata["Value1"],
                "Dictionary meta-data for Foo has unexpected value for 'Value1'.");
            Assert.AreEqual("", metadata["Value2"],
                "Dictionary meta-data for Foo has unexpected value for 'Value2'.");
        }

        [TestMethod]
        public void AutomaticIncrementDeserializationTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,#,""Fred""
Values,Foo,#,""Bert""
Values,Foo,#,""Harry""";
            var options =
                new Options()
                    .ForType<Foo>();
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize.");

            Assert.AreEqual(3, data.Count(), "Unexpected number of records.");
            Assert.AreEqual(1, data.Count(n => n.Name == "Fred"), "Failed to find record for 'Fred'.");
            Assert.AreEqual(1, data.Count(n => n.Name == "Bert"), "Failed to find record for 'Bert'.");
            Assert.AreEqual(1, data.Count(n => n.Name == "Harry"), "Failed to find record for 'Harry'.");

            Assert.AreEqual(1, data.First(n => n.Name == "Fred").Id, "Unexpected Id for the record for 'Fred'.");
            Assert.AreEqual(2, data.First(n => n.Name == "Bert").Id, "Unexpected Id for the record for 'Bert'.");
            Assert.AreEqual(3, data.First(n => n.Name == "Harry").Id, "Unexpected Id for the record for 'Harry'.");

            Assert.AreEqual(1, data.Skip(0).First().Id, "Unexpected Id for first record.");
            Assert.AreEqual(2, data.Skip(1).First().Id, "Unexpected Id for second record.");
            Assert.AreEqual(3, data.Skip(2).First().Id, "Unexpected Id for third record.");
        }

        [TestMethod]
        public void AutomaticIncrementMetadataDeserialization1Test()
        {
            // Arrange
            var text = @"
Metadata,Foo,#,""A""
Properties,Foo,Id,Name
Values,Foo,#,""Fred""
Values,Foo,#,""Bert""";
            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMetadata<Baz>("Metadata", "Id", "Name");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize text.");
            Assert.AreEqual(2, data.Count(n => n.GetType() == typeof(Foo)), "Unexpected number of Foo entities.");

            var fooEntities = 
                data.Where(n => n.GetType() == typeof(Foo));

            Assert.AreEqual(1, fooEntities.Select(foo => foo.Id).Min(), "Unexpected lowest Id of Foo entities.");
            Assert.AreEqual(2, fooEntities.Select(foo => foo.Id).Max(), "Unexpected highest Id of Foo entities.");
            Assert.IsFalse(
                fooEntities
                    .GroupBy(n => n.Id)
                    .Any(n => n.Count() > 1), "Found duplicate Ids for Foo entities.");

            Assert.IsTrue(converter.Metadata.ContainsKey(typeof(Foo).Name), "There is no Foo metadata.");

            var bazMetadata = 
                converter.Metadata[typeof(Foo).Name]
                    .Where(metadata => metadata is Baz)
                    .Cast<Baz>()
                    .ToList();

            Assert.AreEqual(1, bazMetadata.Count, "Unexpected amound of Baz metadata.");
            Assert.AreEqual(1, bazMetadata.Select(baz => baz.Id).Min(), "Unexpected lowest Id of Baz entities.");
            Assert.AreEqual(1, bazMetadata.Select(bar => bar.Id).Max(), "Unexpected highest Id of Baz entities.");
            Assert.IsFalse(
                bazMetadata
                    .GroupBy(n => n.Id)
                    .Any(n => n.Count() > 1), "Found duplicate Ids for Baz metadata.");
        }

        [TestMethod]
        public void AutomaticIncrementMetadataDeserialization2Test()
        {
            // Arrange
            var text = @"
Metadata,Foo,#,""A""
Properties,Foo,Id,Name
Values,Foo,#,""Fred""
Values,Foo,#,""Bert""

Metadata,Bar,#,""B""
Properties,Bar,Id,Name
Values,Bar,#,""Monday""
Values,Bar,#,""Tuesday""";
            var options =
                new Options()
                    .ForType<Foo>()
                    .ForType<Bar>()
                    .ForMetadata<Baz>("Metadata", "Id", "Name");
            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize text.");
            Assert.AreEqual(2, data.Count(n => n.GetType() == typeof(Foo)), "Unexpected number of Foo entities.");
            Assert.AreEqual(2, data.Count(n => n.GetType() == typeof(Bar)), "Unexpected number of Bar entities.");

            var fooEntities = data.Where(n => n.GetType() == typeof(Foo));
            var barEntities = data.Where(n => n.GetType() == typeof(Bar));

            Assert.AreEqual(1, fooEntities.Select(foo => foo.Id).Min(), "Unexpected lowest Id of Foo entities.");
            Assert.AreEqual(2, fooEntities.Select(foo => foo.Id).Max(), "Unexpected highest Id of Foo entities.");
            Assert.IsFalse(
                fooEntities
                    .GroupBy(n => n.Id)
                    .Any(n => n.Count() > 1), "Found duplicate Ids for Foo entities.");

            Assert.AreEqual(1, barEntities.Select(bar => bar.Id).Min(), "Unexpected lowest Id of Bar entities.");
            Assert.AreEqual(2, barEntities.Select(bar => bar.Id).Max(), "Unexpected highest Id of Bar entities.");
            Assert.IsFalse(
                barEntities
                    .GroupBy(n => n.Id)
                    .Any(n => n.Count() > 1), "Found duplicate Ids for Bar entities.");

            var bazMetadata = new List<Baz>();
            foreach (Baz baz in converter.Metadata[typeof(Foo).Name])
                bazMetadata.Add(baz);
            foreach (Baz baz in converter.Metadata[typeof(Bar).Name])
                bazMetadata.Add(baz);

            Assert.AreEqual(2, bazMetadata.Count(), "Unexpected amound of Baz metadata.");
            Assert.AreEqual(1, bazMetadata.Select(baz => baz.Id).Min(), "Unexpected lowest Id of Baz entities.");
            Assert.AreEqual(2, bazMetadata.Select(bar => bar.Id).Max(), "Unexpected highest Id of Baz entities.");
            Assert.IsFalse(
                bazMetadata
                    .GroupBy(n => n.Id)
                    .Any(n => n.Count() > 1), "Found duplicate Ids for Baz metadata.");
        }

        [TestMethod]
        public void DynamicDeserializationSimpleTest()
        {
            DynamicDeserializationImplementation(@"
Properties,FooBar,Id,Name,Value,Thing
Values,FooBar,#,""Fred"",77,Alpha
Values,FooBar,#,""Bert"",99,Beta",
                "FooBar", new[] { "Id", "Name", "Value", "Thing", },
                new[] { "Id", "Name", "Value", "Thing", },
                new[] { "1", "Fred", "77", "Alpha", },
                new[] { "2", "Bert", "99", "Beta", });
        }

        [TestMethod]
        public void DynamicDeserializationExtraPropertiesTest()
        {
            DynamicDeserializationImplementation(@"
Properties,FooBar,Id,Name,Value,Thing,Other
Values,FooBar,#,""Fred"",77,Alpha,""Mike""
Values,FooBar,#,""Bert"",99,Beta,""Oscar""",
                "FooBar", new[] {"Id","Name","Value","Thing",},
                new[] { "Id", "Name", "Value", "Thing", },
                new[] { "1", "Fred", "77", "Alpha", },
                new[] { "2", "Bert", "99", "Beta", });
        }

        [TestMethod]
        public void DynamicDeserializationExtraValuesTest()
        {
            DynamicDeserializationImplementation(@"
Properties,FooBar,Id,Name,Value,Thing
Values,FooBar,#,""Fred"",77,Alpha
Values,FooBar,#,""Bert"",99,Beta",
                "FooBar", new[] { "Id", "Name", "Value", "Thing", "Other", },
                new[] { "Id", "Name", "Value", "Thing", "Other", },
                new[] { "1", "Fred", "77", "Alpha", "", },
                new[] { "2", "Bert", "99", "Beta", "", });
        }

        [TestMethod]
        public void DynamicDeserializationTypelessMetadataTest()
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
            var data = converter.Deserialize(text);

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
        public void DynamicSerializationSimpleTest()
        {
            DynamicSerializationImplemation(
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
        public void TypedReferenceDataDeserializationTest()
        {
            // Assign
            var text = @"
Properties,Foo,Id,Name
Values,Foo,#,""Fred""
Values,Foo,#,""Bert""

Properties,OtherFoo,Id,FooId,Name
Values,OtherFoo,#,#Foo(""Bert""),""Alpha""
Values,OtherFoo,#,#Foo(""Fred""),""Beta""
";
            var options =
                new Options()
                    .ForType<Foo>()
                    .ForType<OtherFoo>();

            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize.");

            var fooData =
                data.Where(n => n.GetType() == typeof(Foo))
                    .Select(n => n as Foo)
                    .NotNull()
                    .ToList();
            var otherFooData=
                data.Where(n => n.GetType() == typeof(OtherFoo))
                    .Select(n => n as OtherFoo)
                    .NotNull()
                    .ToList();

            Assert.AreEqual(2, fooData.Count, "Unexpected number of Foo entities.");
            Assert.AreEqual(2, otherFooData.Count, "Unexpected number of OtherFoo entities.");

            var fred = fooData.FirstOrDefault(n => n.Name == "Fred");
            var bert = fooData.FirstOrDefault(n => n.Name == "Bert");
            var alpha = otherFooData.FirstOrDefault(n => n.Name == "Alpha");
            var beta = otherFooData.FirstOrDefault(n => n.Name == "Beta");
            Assert.IsNotNull(fred ?? bert ?? alpha ?? beta, "Expected records do not exist.");

            Assert.AreEqual(fred?.Id, beta?.FooId, "Failed to reference record: fred.");
            Assert.AreEqual(bert?.Id, alpha?.FooId, "Failed to reference record: bert.");
        }

        [TestMethod]
        public void TypedSelfReferencingDataDeserializationTest()
        {
            // Assign
            var text = @"
Properties,OtherFoo,Id,FooId,Name
Values,OtherFoo,#,#OtherFoo(""Beta""),""Alpha""
Values,OtherFoo,#,#OtherFoo(""Alpha""),""Beta""
";
            var options =
                new Options()
                    .ForType<OtherFoo>();

            var converter = new Converter(options);

            // Act
            var data = converter.Deserialize<Foo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize.");

            var otherFooData =
                data.Where(n => n.GetType() == typeof(OtherFoo))
                    .Select(n => n as OtherFoo)
                    .NotNull()
                    .ToList();

            Assert.AreEqual(2, otherFooData.Count, "Unexpected number of OtherFoo entities.");

            var alpha = otherFooData.FirstOrDefault(n => n.Name == "Alpha");
            var beta = otherFooData.FirstOrDefault(n => n.Name == "Beta");
            Assert.IsNotNull(alpha ?? beta, "Expected records do not exist.");

            Assert.AreEqual(alpha?.Id, beta?.FooId, "Failed to find reference record: alpha.");
            Assert.AreEqual(beta?.Id, alpha?.FooId, "Failed to find reference record: beta.");
        }

        [TestMethod]
        public void TypelessReferenceDataDeserializationTest()
        {
            // Assign
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
            // Assign
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
            // Assign
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
                var item1Value=source.Item2.FirstOrDefault(item => item[index] == item1Name);
                var item2Value = source.Item2.FirstOrDefault(item => item[index] == item2Name);

                Assert.IsNotNull(item1Value, "Failed to find record: {0}", item1Name);
                Assert.IsNotNull(item2Value, "Failed to find record: {0}", item2Name);

                return (item1Value, item2Value);
            }

            int getIndex((string[], IEnumerable<string[]>) source, string indexName) => 
                source.Item1.ToList().IndexOf(indexName);
        }

        #region Support routines

        private static void DynamicDeserializationImplementation(string text,
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

        private static void DynamicSerializationImplemation(Dictionary<string, (string[], IEnumerable<string[]>)> data,
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

        #region Model classes

        public enum TestEnum
        {
            None = 0,

            Value,
            Data,
            Entity,
        }

        // WARNING: Don't change the names of the Foo, Bar and Baz classes, their names are tied
        // to the textual data in some of the tests and those tests will then fail.

        private class Foo
        {
            public int Id { get; set; }
            public string? Name { get; set; }

            public TestEnum TestEnum { get; set; }
        }

        private class Bar : Foo
        {
            public bool Flag { get; set; }
            public double Value { get; set; }
            public decimal Number { get; set; }
        }

        private class Baz : Foo
        {
            public string? Code { get; set; }
            public string? Description { get; set; }
        }

        private class Metadata
        {
            public string? Name { get; set; }
            public int Number { get; set; }
        }

        [AttributeUsage(AttributeTargets.Class)]
        private class MetadataAttribute : Attribute
        {
            public string? Name { get; set; }
            public int Value { get; set; }
        }

        [CsvConverterClass("Foo")]
        private class AttrFoo
        {
            [CsvConverterProperty("Id")]
            public int Identity { get; set; }

            [CsvConverterProperty("Name")]
            public string? FullName { get; set; }

            [CsvConverterProperty("TestEnum")]
            public TestEnum TestEnumValue { get; set; }
        }

        private class OtherFoo : Foo
        {
            public int FooId { get; set; }
        }

        #endregion
    }
}