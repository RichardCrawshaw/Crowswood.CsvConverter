using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class ConverterSerializeTests
    {
        private readonly bool logSerializedOutput = false;

        // Note the `\r?$` on the end of the pattern. This is due to `$` matching either `\n` 
        // or the end of the input string, but NOT matching `\r`, AND under Windows there 
        // being `\r\n` as the line terminator. See:
        // https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-options#multiline-mode
        private const string PATTERN = "^{0}\r?$";
        private const string EXPECTED_MESSAGE = "Failed to find {0}.";
        private const string UNWANTED_MESSAGE = "Found {0}.";

        [TestMethod]
        public void NoDataTest()
        {
            // Arrange
            var options =
                Options.None;
            var converter = new Converter(options);

            // Act
            var text =
                converter
                    .Serialize()
                    .ToString();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");
            Assert.IsTrue(string.IsNullOrEmpty(text),
                "Serialization with no data does not produce empty result.");
        }

        [TestMethod]
        public void BlankLineTest()
        {
            // Arrange
            var options =
                Options.None;
            var converter = new Converter(options);

            // Act
            var text =
                converter
                    .Serialize()
                    .BlankLine(2)
                    .ToString();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            Assert.IsFalse(string.IsNullOrEmpty(text), "Serialize two blank lines produced empty text.");
            Assert.IsTrue(string.IsNullOrWhiteSpace(text), "Serialize two blank line did not produced whitespace.");
        }

        [TestMethod]
        public void CommentTest()
        {
            // Arrange
            var options =
                Options.None;
            var converter = new Converter(options);

            var prefix = "#";
            var comment = "This is a comment.";

            // Act
            var text =
                converter
                    .Serialize()
                    .Comment(prefix, comment)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Comment"] = "# This is a comment.",
            };

            AssertResult(text, 1, 1, checks);
        }

        [TestMethod]
        public void EmptyGlobalConfigTest()
        {
            // Arrange
            var options =
                Options.None;
            var converter = new Converter(options);
            var config =
                new Dictionary<string, string>();

            // Act
            var text =
                converter
                    .Serialize()
                    .GlobalConfig(config)
                    .ToString();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            Assert.IsTrue(string.IsNullOrEmpty(text), "No global config gives non-empty result.");
        }

        [TestMethod]
        public void GlobalConfigTest()
        {
            // Arrange
            var options =
                Options.None;
            var converter = new Converter(options);

            var globalConfig =
                new Dictionary<string, string>
                {
                    [Converter.Keys.GlobalConfig.ConversionTypePrefix] = "AAA",
                    [Converter.Keys.GlobalConfig.ConversionValuePrefix] = "BBB",
                    [Converter.Keys.GlobalConfig.PropertyPrefix] = "CCC",
                    [Converter.Keys.GlobalConfig.ReferenceIdColumnName] = "DDD",
                    [Converter.Keys.GlobalConfig.ReferenceNameColumnName] = "EEE",
                    [Converter.Keys.GlobalConfig.ValuesPrefix] = "FFF",
                    ["Rubbish"] = "aaa",
                };

            // Act
            var text =
                converter
                    .Serialize()
                    .GlobalConfig(globalConfig)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["GlobalConfig ConversionTypePrefix"] = "GlobalConfig,ConversionTypePrefix,AAA",
                ["GlobalConfig ConversionValuePrefix"] = "GlobalConfig,ConversionValuePrefix,BBB",
                ["GlobalConfig PropertyPrefix"] = "GlobalConfig,PropertyPrefix,CCC",
                ["GlobalConfig ReferenceIdColumnName"] = "GlobalConfig,ReferenceIdColumnName,DDD",
                ["GlobalConfig ReferenceNameColumnName"] = "GlobalConfig,ReferenceNameColumnName,EEE",
                ["GlobalConfig ValuesPrefix"] = "GlobalConfig,ValuesPrefix,FFF",
            };

            Dictionary<string, string> unwanted = new()
            {
                ["Invalid GlobalConfig"] = "GlobalConfig,Rubbish,.*",
            };

            AssertResult(text, checks.Count, 0, checks, unwanted);
        }

        [TestMethod]
        public void TypedConfigTest()
        {
            // Arrange
            var options =
                Options.None;
            var converter = new Converter(options);

            var typedConfig =
                new Dictionary<string, string>
                {
                    [Converter.Keys.TypedConfig.ConversionTypePrefix] = "AAA",
                    [Converter.Keys.TypedConfig.ConversionValuePrefix] = "BBB",
                    [Converter.Keys.TypedConfig.PropertyPrefix] = "CCC",
                    [Converter.Keys.TypedConfig.ReferenceIdColumnName] = "DDD",
                    [Converter.Keys.TypedConfig.ReferenceNameColumnName] = "EEE",
                    [Converter.Keys.TypedConfig.ValuesPrefix] = "FFF",
                    ["Rubbish"] = "aaa",
                };

            // Act
            var text =
                converter
                    .Serialize()
                    .TypeConfig<Foo>(typedConfig)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["TypedConfig Foo ConversionTypePrefix"] = "TypedConfig,Foo,ConversionTypePrefix,AAA",
                ["TypedConfig Foo ConversionValuePrefix"] = "TypedConfig,Foo,ConversionValuePrefix,BBB",
                ["TypedConfig Foo PropertyPrefix"] = "TypedConfig,Foo,PropertyPrefix,CCC",
                ["TypedConfig Foo ReferenceIdColumnName"] = "TypedConfig,Foo,ReferenceIdColumnName,DDD",
                ["TypedConfig Foo ReferenceNameColumnName"] = "TypedConfig,Foo,ReferenceNameColumnName,EEE",
                ["TypedConfig Foo ValuesPrefix"] = "TypedConfig,Foo,ValuesPrefix,FFF",
            };

            Dictionary<string, string> unwanted = new()
            {
                ["Invalid GlobalConfig"] = "TypedConfig,Foo,Rubbish,.*",
            };

            AssertResult(text, checks.Count, 0, checks, unwanted);
        }

        [TestMethod]
        public void TypeConversionTest()
        {
            // Arrange
            var options = Options.None;
            var converter = new Converter(options);

            var typeConversion =
                new Dictionary<string, string>
                {
                    ["Foo"] = "Bar",
                };

            // Act
            var text =
                converter
                    .Serialize()
                    .TypeConversion(typeConversion)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["TypeConversion 'Foo' to 'Bar'"] = "ConversionTypePrefix,\"Foo\",\"Bar\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void ValueConversionTest()
        {
            // Arrange
            var options = Options.None;
            var converter = new Converter(options);

            var valueConversion =
                new Dictionary<string, string>
                {
                    ["Foo"] = "Bar",
                };

            // Act
            var text =
                converter
                    .Serialize()
                    .ValueConversion(valueConversion)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["ValueConversion 'Foo' to 'Bar'"] = "ConversionValuePrefix,\"Foo\",\"Bar\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void TypedMetadataTest()
        {
            // Arrange
            const string metadataPrefix = "Metadata";
            var propertyNames = new[] { "Name", "Number", };
            var options =
                new Options()
                    .ForMetadata<SomeMetadata>(metadataPrefix, propertyNames[0], propertyNames[1]);
            var converter = new Converter(options);

            var metadata =
                new List<SomeMetadata>
                {
                    new SomeMetadata{ Name = "ABC", Number = 1, },
                    new SomeMetadata{ Name = "XYZ", Number = 99, },
                };

            // Act
            var text =
                converter
                    .Serialize()
                    .TypedMetadata<SomeMetadata, Foo>(metadata)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Foo Metadata 'ABC'"] = "Metadata,Foo,\"ABC\",1",
                ["Foo Metadata 'XYZ'"] = "Metadata,Foo,\"XYZ\",99",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void TypelessMetadataTest()
        {
            // Arrange
            var prefix = "typelessMD";
            var propertyNames = new[] { "Name", "Number", };
            var options =
                new Options()
                    .ForMetadata(prefix, true, propertyNames);
            var converter = new Converter(options);

            var metadata =
                new Dictionary<string, string>
                {
                    [propertyNames[0]] = "Fred",
                    [propertyNames[1]] = 77.ToString(),
                };

            // Act
            var text =
                converter
                    .Serialize()
                    .TypelessMetadata("Foo", prefix, metadata)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Typeless Metadata"] = "typelessMD,Foo,\"Fred\",\"77\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void TypedDataTest()
        {
            // Arrange
            var options = Options.None;
            var converter = new Converter(options);

            var fooData =
                new List<Foo>
                {
                    new Foo { Id = 1, Name = "Fred", Value = "East", },
                    new Foo { Id = 2, Name = "Bert", Value = "West", },
                };

            // Act
            var text =
                converter
                    .Serialize()
                    .TypedData(fooData)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Foo properties"] = "Properties,Foo,Id,Name,Value",
                ["Foo value 1"] = "Values,Foo,1,\"Fred\",\"East\"",
                ["Foo value 2"] = "Values,Foo,2,\"Bert\",\"West\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void TypelessDataTest()
        {
            // Arrange
            var options =
                new Options()
                    .ForType<Foo>();
            var converter = new Converter(options);

            var names = new[] { "Id", "Name", "Value", };
            List<string[]> values = new()
            {
                new[] { "1", "Fred", "East", },
                new[] { "2", "Bert", "West", }
            };

            // Act
            var text =
                converter
                    .Serialize()
                    .TypelessData("SomeDataType", names, values)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Properties"] = "Properties,SomeDataType,Id,Name,Value",
                ["Value 1"] = "Values,SomeDataType,\"1\",\"Fred\",\"East\"",
                ["Value 2"] = "Values,SomeDataType,\"2\",\"Bert\",\"West\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void FullTypedTest()
        {
            // Arrange
            const string metadataPrefix = "Metadata";
            var metadataPropertyNames = new[] { "Name", "Number", };
            var options =
                new Options()
                    .ForMetadata<SomeMetadata>(metadataPrefix, metadataPropertyNames[0], metadataPropertyNames[1])
                    .ForType<Foo>();
            var converter = new Converter(options);

            var openingComment = "This is the Full Typed Test.";
            var globalConfigComment = "Here is the global configuration.";
            Dictionary<string, string> globalConfiguration = new()
            {
                [Converter.Keys.GlobalConfig.ConversionTypePrefix] = "AAA",
                [Converter.Keys.GlobalConfig.ConversionValuePrefix] = "BBB",
                [Converter.Keys.GlobalConfig.PropertyPrefix] = "CCC",
                [Converter.Keys.GlobalConfig.ReferenceIdColumnName] = "DDD",
                [Converter.Keys.GlobalConfig.ReferenceNameColumnName] = "EEE",
                [Converter.Keys.GlobalConfig.ValuesPrefix] = "FFF",
            };
            var typeConfigComment = "Here is the type configuration.";
            Dictionary<string, string> fooTypeConfiguration = new()
            {
                [Converter.Keys.TypedConfig.ConversionTypePrefix] = "aaa",
                [Converter.Keys.TypedConfig.ConversionValuePrefix] = "bbb",
                [Converter.Keys.TypedConfig.PropertyPrefix] = "ccc",
                [Converter.Keys.TypedConfig.ReferenceIdColumnName] = "ddd",
                [Converter.Keys.TypedConfig.ReferenceNameColumnName] = "eee",
                [Converter.Keys.TypedConfig.ValuesPrefix] = "fff",
            };
            var typeConversionComment = "Here are some type conversions.";
            Dictionary<string, string> typeConversion = new()
            {
                ["Foo"] = "Bar",
            };
            var valueConversionComment = "Here are some value conversions.";
            Dictionary<string, string> valueConversion = new()
            {
                ["Foo"] = "Bar",
            };
            var fooDataComment = "Here is the Foo data preceeded by some metadata.";
            List<SomeMetadata> fooMetadata = new()
            {
                new SomeMetadata{ Name = "ABC", Number = 1, },
                new SomeMetadata{ Name = "XYZ", Number = 99, },
            };
            List<Foo> fooData = new()
            {
                new Foo { Id = 1, Name = "Fred", Value = "East", },
                new Foo { Id = 2, Name = "Bert", Value = "West", },
            };

            // Act
            var text =
                converter
                    .Serialize()
                    .Comment("#", openingComment)
                    .BlankLine()
                    .Comment("#", globalConfigComment)
                    .GlobalConfig(globalConfiguration)
                    .BlankLine()
                    .Comment("#", typeConfigComment)
                    .TypeConfig<Foo>(fooTypeConfiguration)
                    .BlankLine()
                    .Comment("#", typeConversionComment)
                    .TypeConversion(typeConversion)
                    .BlankLine()
                    .Comment("#", valueConversionComment)
                    .ValueConversion(valueConversion)
                    .BlankLine()
                    .Comment("#", fooDataComment)
                    .TypedMetadata<SomeMetadata, Foo>(fooMetadata)
                    .TypedData(fooData)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Opening comment"] = "# This is the Full Typed Test.",

                ["Global config comment"] = "# Here is the global configuration.",
                ["GlobalConfig ConversionTypePrefix"] = "GlobalConfig,ConversionTypePrefix,AAA",
                ["GlobalConfig ConversionValuePrefix"] = "GlobalConfig,ConversionValuePrefix,BBB",
                ["GlobalConfig PropertyPrefix"] = "GlobalConfig,PropertyPrefix,CCC",
                ["GlobalConfig ReferenceIdColumnName"] = "GlobalConfig,ReferenceIdColumnName,DDD",
                ["GlobalConfig ReferenceNameColumnName"] = "GlobalConfig,ReferenceNameColumnName,EEE",
                ["GlobalConfig ValuesPrefix"] = "GlobalConfig,ValuesPrefix,FFF",

                ["Type config comment"] = "# Here is the type configuration.",
                ["TypedConfig Foo ConversionTypePrefix"] = "TypedConfig,Foo,ConversionTypePrefix,aaa",
                ["TypedConfig Foo ConversionValuePrefix"] = "TypedConfig,Foo,ConversionValuePrefix,bbb",
                ["TypedConfig Foo PropertyPrefix"] = "TypedConfig,Foo,PropertyPrefix,ccc",
                ["TypedConfig Foo ReferenceIdColumnName"] = "TypedConfig,Foo,ReferenceIdColumnName,ddd",
                ["TypedConfig Foo ReferenceNameColumnName"] = "TypedConfig,Foo,ReferenceNameColumnName,eee",
                ["TypedConfig Foo ValuesPrefix"] = "TypedConfig,Foo,ValuesPrefix,fff",

                ["Type conversion comment"] = "# Here are some type conversions.",
                ["TypeConversion 'Foo' to 'Bar'"] = "ConversionTypePrefix,\"Foo\",\"Bar\"",

                ["Value conversion comment"] = "# Here are some value conversions.",
                ["ValueConversion 'Foo' to 'Bar'"] = "ConversionValuePrefix,\"Foo\",\"Bar\"",

                ["Foo data comment"] = "# Here is the Foo data preceeded by some metadata.",
                ["Foo Metadata 'ABC'"] = "Metadata,Foo,\"ABC\",1",
                ["Foo Metadata 'XYZ'"] = "Metadata,Foo,\"XYZ\",99",
                ["Foo properties"] = "ccc,Foo,Id,Name,Value",
                ["Foo value 1"] = "fff,Foo,1,\"Fred\",\"East\"",
                ["Foo value 2"] = "fff,Foo,2,\"Bert\",\"West\"",
            };

            var expectedLineCount =
                1 + // opening comment
                1 + // blank line
                1 + // global config comment
                globalConfiguration.Count +
                1 + // blank line
                1 + // type config comment
                fooTypeConfiguration.Count +
                1 + // blank line
                1 + // type conversion comment
                typeConversion.Count +
                1 + // blank line
                1 + // value conversion comment
                valueConversion.Count +
                1 + // blank line
                1 + // foo data comment
                fooMetadata.Count +
                1 + // foo properties
                fooData.Count;
            var expectedNumberOfCommentLines =
                1 + // opening comment
                1 + // global config comment
                1 + // type config comment
                1 + // type conversion comment
                1 + // value conversion comment
                1 + // foo data comment
                0;

            AssertResult(text, expectedLineCount, expectedNumberOfCommentLines, checks);
        }

        [TestMethod]
        public void FullTypelessTest()
        {
            // Arrange
            const string metadataPrefix = "Metadata";
            var metadataPropertyNames = new[] { "Name", "Number", };
            const string fooName = "Foo";
            var fooPropertyNames = new[] { "Id", "Name", "Value", };
            var options =
                new Options()
                    .ForMetadata(metadataPrefix, allowNulls: true, metadataPropertyNames[0], metadataPropertyNames[1])
                    .ForType(fooName, fooPropertyNames[0], fooPropertyNames[1..]);
            var converter = new Converter(options);

            var openingComment = "This is the Full Typeless Test.";
            var globalConfigComment = "Here is the global configuration.";
            Dictionary<string, string> globalConfiguration = new()
            {
                [Converter.Keys.GlobalConfig.ConversionTypePrefix] = "AAA",
                [Converter.Keys.GlobalConfig.ConversionValuePrefix] = "BBB",
                [Converter.Keys.GlobalConfig.PropertyPrefix] = "CCC",
                [Converter.Keys.GlobalConfig.ReferenceIdColumnName] = "DDD",
                [Converter.Keys.GlobalConfig.ReferenceNameColumnName] = "EEE",
                [Converter.Keys.GlobalConfig.ValuesPrefix] = "FFF",
            };
            var typeConfigComment = "Here is the type configuration.";
            Dictionary<string, string> fooTypeConfiguration = new()
            {
                [Converter.Keys.TypedConfig.ConversionTypePrefix] = "aaa",
                [Converter.Keys.TypedConfig.ConversionValuePrefix] = "bbb",
                [Converter.Keys.TypedConfig.PropertyPrefix] = "ccc",
                [Converter.Keys.TypedConfig.ReferenceIdColumnName] = "ddd",
                [Converter.Keys.TypedConfig.ReferenceNameColumnName] = "eee",
                [Converter.Keys.TypedConfig.ValuesPrefix] = "fff",
            };
            var typeConversionComment = "Here are some type conversions.";
            Dictionary<string, string> typeConversion = new()
            {
                ["Foo"] = "Bar",
            };
            var valueConversionComment = "Here are some value conversions.";
            Dictionary<string, string> valueConversion = new()
            {
                ["Foo"] = "Bar",
            };
            var fooDataComment = "Here is the Foo data preceeded by some metadata.";
            Dictionary<string, string> fooMetadata = new()
            {
                [metadataPropertyNames[0]] = "Harry",
                [metadataPropertyNames[1]] = 77.ToString(),
            };
            List<string[]> fooData = new()
            {
                new [] { 1.ToString(), "Fred", "East", },
                new [] { 2.ToString(), "Bert", "West", },
            };

            // Act
            var text =
                converter
                    .Serialize()
                    .Comment("#", openingComment)
                    .BlankLine()
                    .Comment("#", globalConfigComment)
                    .GlobalConfig(globalConfiguration)
                    .BlankLine()
                    .Comment("#", typeConfigComment)
                    .TypeConfig<Foo>(fooTypeConfiguration)
                    .BlankLine()
                    .Comment("#", typeConversionComment)
                    .TypeConversion(typeConversion)
                    .BlankLine()
                    .Comment("#", valueConversionComment)
                    .ValueConversion(valueConversion)
                    .BlankLine()
                    .Comment("#", fooDataComment)
                    .TypelessMetadata(fooName, metadataPrefix, fooMetadata)
                    .TypelessData(fooName, fooPropertyNames, fooData)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Opening comment"] = "# This is the Full Typeless Test.",

                ["Global config comment"] = "# Here is the global configuration.",
                ["GlobalConfig ConversionTypePrefix"] = "GlobalConfig,ConversionTypePrefix,AAA",
                ["GlobalConfig ConversionValuePrefix"] = "GlobalConfig,ConversionValuePrefix,BBB",
                ["GlobalConfig PropertyPrefix"] = "GlobalConfig,PropertyPrefix,CCC",
                ["GlobalConfig ReferenceIdColumnName"] = "GlobalConfig,ReferenceIdColumnName,DDD",
                ["GlobalConfig ReferenceNameColumnName"] = "GlobalConfig,ReferenceNameColumnName,EEE",
                ["GlobalConfig ValuesPrefix"] = "GlobalConfig,ValuesPrefix,FFF",

                ["Type config comment"] = "# Here is the type configuration.",
                ["TypedConfig Foo ConversionTypePrefix"] = "TypedConfig,Foo,ConversionTypePrefix,aaa",
                ["TypedConfig Foo ConversionValuePrefix"] = "TypedConfig,Foo,ConversionValuePrefix,bbb",
                ["TypedConfig Foo PropertyPrefix"] = "TypedConfig,Foo,PropertyPrefix,ccc",
                ["TypedConfig Foo ReferenceIdColumnName"] = "TypedConfig,Foo,ReferenceIdColumnName,ddd",
                ["TypedConfig Foo ReferenceNameColumnName"] = "TypedConfig,Foo,ReferenceNameColumnName,eee",
                ["TypedConfig Foo ValuesPrefix"] = "TypedConfig,Foo,ValuesPrefix,fff",

                ["Type conversion comment"] = "# Here are some type conversions.",
                ["TypeConversion 'Foo' to 'Bar'"] = "ConversionTypePrefix,\"Foo\",\"Bar\"",

                ["Value conversion comment"] = "# Here are some value conversions.",
                ["ValueConversion 'Foo' to 'Bar'"] = "ConversionValuePrefix,\"Foo\",\"Bar\"",

                ["Foo data comment"] = "# Here is the Foo data preceeded by some metadata.",
                ["Foo Metadata 'Harry'"] = "Metadata,Foo,\"Harry\",\"77\"",
                ["Foo properties"] = "ccc,Foo,Id,Name,Value",
                ["Foo value 1"] = "fff,Foo,\"1\",\"Fred\",\"East\"",
                ["Foo value 2"] = "fff,Foo,\"2\",\"Bert\",\"West\"",
            };

            var expectedLineCount =
                1 + // opening comment
                1 + // blank line
                1 + // global config comment
                globalConfiguration.Count +
                1 + // blank line
                1 + // type config comment
                fooTypeConfiguration.Count +
                1 + // blank line
                1 + // type conversion comment
                typeConversion.Count +
                1 + // blank line
                1 + // value conversion comment
                valueConversion.Count +
                1 + // blank line
                1 + // foo data comment
                1 + // fooMetadata
                1 + // foo properties
                fooData.Count;
            var expectedNumberOfCommentLines =
                1 + // opening comment
                1 + // global config comment
                1 + // type config comment
                1 + // type conversion comment
                1 + // value conversion comment
                1 + // foo data comment
                0;

            AssertResult(text, expectedLineCount, expectedNumberOfCommentLines, checks);
        }

        [TestMethod]
        public void AttributeObjectDataFluentTest()
        {
            // Arrange
            var options =
                new Options()
                    .ForType<AttrFoo>();
            var converter = new Converter(options);

            List<AttrFoo> data = new()
            {
                new AttrFoo{ Id = 1, Name = "Fred", Value = "East", },
                new AttrFoo{ Id = 2, Name = "Bert", Value = "West", },
            };

            // Act
            var text =
                converter
                    .Serialize()
                    .Comment("#", "This is the Attribute Data Test.")
                    .TypedData(data)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Opening comment"] = "# This is the Attribute Data Test.",

                ["Foo Properties"] = "Properties,FooObj,Identity,FullName,Val",
                ["Foo Values 1"] = "Values,FooObj,1,\"Fred\",\"East\"",
                ["Foo Values 2"] = "Values,FooObj,2,\"Bert\",\"West\"",
            };

            var expectedLineCount =
                1 + // Opening comment
                1 + // Data properties
                data.Count;

            var expectedNumberOfCommentLines = 1;

            AssertResult(text, expectedLineCount, expectedNumberOfCommentLines, checks);
        }

        [TestMethod]
        public void AttributeMetadataFluentTest()
        {
            // Arrange
            var options =
                new Options()
                    .ForType<AttrFoo>()
                    .ForMetadata<AttrMetadata>("MD", "Name", "Number");
            var converter = new Converter(options);

            List<AttrMetadata> metadata = new()
            {
                new AttrMetadata{ Name = "North", Number = 5, },
            };

            List<AttrFoo> data = new()
            {
                new AttrFoo{ Id = 1, Name = "Fred", Value = "East", },
                new AttrFoo{ Id = 2, Name = "Bert", Value = "West", },
            };

            // Act
            var text =
                converter
                    .Serialize()
                    .TypedMetadata<AttrMetadata, AttrFoo>(metadata)
                    .TypedData(data)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Foo metadata 1"] = "MD,FooObj,\"North\",5",
                ["Foo Properties"] = "Properties,FooObj,Identity,FullName,Val",
                ["Foo Values 1"] = "Values,FooObj,1,\"Fred\",\"East\"",
                ["Foo Values 2"] = "Values,FooObj,2,\"Bert\",\"West\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void AttributeMetadataOptionsTest()
        {
            // Arrange
            List<AttrMetadata> metadata = new()
            {
                new AttrMetadata{ Name = "North", Number = 5, },
            };

            List<AttrFoo> data = new()
            {
                new AttrFoo{ Id = 1, Name = "Fred", Value = "East", },
                new AttrFoo{ Id = 2, Name = "Bert", Value = "West", },
            };

            var options =
                new Options()
                    .ForType<AttrFoo>()
                    .ForMetadata<AttrMetadata>("MD", "Name", "Number")
                    .Serialize(serialize =>
                        serialize
                            .TypedMetadata<AttrMetadata, AttrFoo>(metadata));
            var converter = new Converter(options);

            // Act
            var text =
                converter
                    .Serialize()
                    .TypedData(data)
                    .ToString();

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Foo metadata 1"] = "MD,FooObj,\"North\",5",
                ["Foo Properties"] = "Properties,FooObj,Identity,FullName,Val",
                ["Foo Values 1"] = "Values,FooObj,1,\"Fred\",\"East\"",
                ["Foo Values 2"] = "Values,FooObj,2,\"Bert\",\"West\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        [TestMethod]
        public void ObjectDataWithMetadataAsAttributeTest()
        {
            // Arrange
            List<Foo> data = new()
            {
                new Foo{ Id = 1, Name = "Fred", Value = "East", },
                new Foo{ Id = 2, Name = "Bert", Value = "West", },
            };

            List<MetadataAttribute> metadata = new()
            {
                new MetadataAttribute{ Name = "XYZ", Number = 77, },
            };
            TypeDescriptor.AddAttributes(typeof(Foo), metadata.ToArray());

            var options =
                new Options()
                    .ForType<Foo>()
                    .ForMetadata<MetadataAttribute>("Metadata", "Name", "Number");
            var converter = new Converter(options);

            // Act
            var text =
                converter
                    .Serialize()
                    .TypedData(data)
                    .ToString();

            //Metadata,Foo,"XYZ",77
            //Properties,Foo,Id,Name,Value
            //Values,Foo,1,"Fred","East"
            //Values,Foo,2,"Bert","West"

            // Assert
            Dictionary<string, string> checks = new()
            {
                ["Metadata"] = "Metadata,Foo,\"XYZ\",77",
                ["Foo Properties"] = "Properties,Foo,Id,Name,Value",
                ["Foo Values 1"] = "Values,Foo,1,\"Fred\",\"East\"",
                ["Foo Values 2"] = "Values,Foo,2,\"Bert\",\"West\"",
            };

            AssertResult(text, checks.Count, 0, checks);
        }

        #region Support routines

        private void AssertResult(string text, int lineCount, int commentCount, Dictionary<string, string> checks, Dictionary<string, string>? unwanted = null)
        {
            const string COMMENT_REGEX = "(?:\r?\n){0,2}# ";

            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            var lines = text.Split(Environment.NewLine);

            Assert.AreEqual(lineCount, lines.Length,
                "Unexpected number of lines.");

            Assert.AreEqual(commentCount, Regex.Matches(text, COMMENT_REGEX).Count,
                "Unexpected number of comments.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }

            if (unwanted is not null)
            {
                foreach (var check in unwanted)
                {
                    var pattern = string.Format(PATTERN, check.Value);
                    Assert.IsFalse(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                        UNWANTED_MESSAGE, check.Key);
                }
            }
        }

        #endregion

        #region Model

        private class Foo
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public string? Value { get; set; }
        }

        [CsvConverterClass("FooObj")]
        private class AttrFoo
        {
            [CsvConverterProperty("Identity")]
            public int Id { get; set; }

            [CsvConverterProperty("FullName")]
            public string? Name { get; set; }

            [CsvConverterProperty("Val")]
            public string? Value { get; set; }
        }

        private class SomeMetadata
        {
            public string? Name { get; set; }

            public int Number { get; set; }
        }

        [CsvConverterClass("MetaData")]
        private class AttrMetadata
        {
            [CsvConverterProperty("FullName")]
            public string? Name { get; set; }

            [CsvConverterProperty("Num")]
            public int Number { get; set; }
        }

        [AttributeUsage(AttributeTargets.Class)]
        private class MetadataAttribute : Attribute
        {
            public string? Name { get; set; }

            public int Number { get; set; }
        }

        #endregion
    }
}
