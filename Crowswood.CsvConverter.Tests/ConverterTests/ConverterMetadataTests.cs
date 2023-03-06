using System.ComponentModel;
using Crowswood.CsvConverter.Extensions;

namespace Crowswood.CsvConverter.Tests.ConverterTests
{
    [TestClass]
    public class ConverterMetadataTests : ConverterBaseTests
    {
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
                    .SetPrefixes(propertiesPrefix: "bad",
                                 valuesPrefix: "bad");
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

            Assert.IsTrue(converter.Metadata.ContainsKey(typeof(Foo).Name), "No Foo metadata.");
            Assert.IsTrue(converter.Metadata.ContainsKey(typeof(Bar).Name), "No Bar metadata.");

            var bazMetadata = new List<Baz>();
            foreach (var baz in converter.Metadata[typeof(Foo).Name].Cast<Baz>())
                bazMetadata.Add(baz);
            foreach (var baz in converter.Metadata[typeof(Bar).Name].Cast<Baz>())
                bazMetadata.Add(baz);

            Assert.AreEqual(2, bazMetadata.Count, "Unexpected amound of Baz metadata.");
            Assert.AreEqual(1, bazMetadata.Select(baz => baz.Id).Min(), "Unexpected lowest Id of Baz entities.");
            Assert.AreEqual(2, bazMetadata.Select(bar => bar.Id).Max(), "Unexpected highest Id of Baz entities.");
            Assert.IsFalse(
                bazMetadata
                    .GroupBy(n => n.Id)
                    .Any(n => n.Count() > 1), "Found duplicate Ids for Baz metadata.");
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
            _ = converter.Deserialize<Foo>(text).ToList();

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
            _ = converter.Deserialize<Foo>(text).ToList();

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

        #region Model classes

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

        #endregion
    }
}
