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
            Assert.IsTrue(converter.Metadata.ContainsKey(typeof(Foo)), "The metadata for Foo not available via converter.");
            Assert.IsTrue(converter.Metadata[typeof(Foo)].Any(), "Foo has an empty list of metadata.");
            Assert.IsTrue(converter.Metadata[typeof(Foo)].Any(md => md is Metadata), 
                "Foo does not have the metadata of type Metadata.");
            Assert.IsTrue(
                converter.Metadata[typeof(Foo)]
                    .NotNull<object, Metadata>()
                    .First() is 
                { 
                    Name: "Simple metadata", 
                    Number: 77, 
                }, "Foo does not have the expected Metadata values.");

            var attributes = TypeDescriptor.GetAttributes(typeof(Foo));

            Assert.IsTrue(attributes.OfType<MetadataAttribute>().Any(), "MetadataAttribute not available via Type.");
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

        private class AttrBar : AttrFoo
        {

        }

        private class AttrBaz : AttrFoo
        {

        }

        #endregion
    }
}