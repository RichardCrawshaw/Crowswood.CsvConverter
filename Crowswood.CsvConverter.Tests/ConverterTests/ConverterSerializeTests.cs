using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Crowswood.CsvConverter.Tests.ConverterTests
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
                    .Serialize();

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
                    .Serialize();

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
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["Comment"] = "# This is a comment.",
            };

            Assert.AreEqual(checks.Count, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
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
                    .Serialize();

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
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["GlobalConfig ConversionTypePrefix"] = "GlobalConfig,ConversionTypePrefix,AAA",
                ["GlobalConfig ConversionValuePrefix"] = "GlobalConfig,ConversionValuePrefix,BBB",
                ["GlobalConfig PropertyPrefix"] = "GlobalConfig,PropertyPrefix,CCC",
                ["GlobalConfig ReferenceIdColumnName"] = "GlobalConfig,ReferenceIdColumnName,DDD",
                ["GlobalConfig ReferenceNameColumnName"] = "GlobalConfig,ReferenceNameColumnName,EEE",
                ["GlobalConfig ValuesPrefix"] = "GlobalConfig,ValuesPrefix,FFF",
            };

            Assert.AreEqual(checks.Count, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }

            Dictionary<string, string> unwanted = new()
            {
                ["Invalid GlobalConfig"] = "GlobalConfig,Rubbish,.*",
            };

            foreach (var check in unwanted)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsFalse(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    UNWANTED_MESSAGE, check.Key);
            }
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
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["TypedConfig Foo ConversionTypePrefix"] = "TypedConfig,Foo,ConversionTypePrefix,AAA",
                ["TypedConfig Foo ConversionValuePrefix"] = "TypedConfig,Foo,ConversionValuePrefix,BBB",
                ["TypedConfig Foo PropertyPrefix"] = "TypedConfig,Foo,PropertyPrefix,CCC",
                ["TypedConfig Foo ReferenceIdColumnName"] = "TypedConfig,Foo,ReferenceIdColumnName,DDD",
                ["TypedConfig Foo ReferenceNameColumnName"] = "TypedConfig,Foo,ReferenceNameColumnName,EEE",
                ["TypedConfig Foo ValuesPrefix"] = "TypedConfig,Foo,ValuesPrefix,FFF",
            };

            Assert.AreEqual(checks.Count, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }

            Dictionary<string, string> unwanted = new()
            {
                ["Invalid GlobalConfig"] = "TypedConfig,Foo,Rubbish,.*",
            };

            foreach (var check in unwanted)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsFalse(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    UNWANTED_MESSAGE, check.Key);
            }
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
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["TypeConversion 'Foo' to 'Bar'"] = "ConversionTypePrefix,\"Foo\",\"Bar\"",
            };

            Assert.AreEqual(checks.Count, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
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
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["ValueConversion 'Foo' to 'Bar'"] = "ConversionValuePrefix,\"Foo\",\"Bar\"",
            };

            Assert.AreEqual(checks.Count, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
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
                    .TypedMetadata<Foo, SomeMetadata>(metadata)
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["Foo Metadata 'ABC'"] = "Metadata,Foo,\"ABC\",1",
                ["Foo Metadata 'XYZ'"] = "Metadata,Foo,\"XYZ\",99",
            };

            Assert.AreEqual(checks.Count, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
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
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["Typeless Metadata"] = "typelessMD,Foo,\"Fred\",\"77\"",
            };

            Assert.AreEqual(checks.Count, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
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
                    .TypedData<Foo>(fooData)
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["Foo properties"] = "Properties,Foo,Id,Name,Value",
                ["Foo value 1"] = "Values,Foo,1,\"Fred\",\"East\"",
                ["Foo value 2"] = "Values,Foo,2,\"Bert\",\"West\"",
            };

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
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
            var text=
                converter
                    .Serialize()
                    .TypelessData("SomeDataType", names, values)
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

            Dictionary<string, string> checks = new()
            {
                ["Properties"] = "Properties,SomeDataType,Id,Name,Value",
                ["Value 1"] = "Values,SomeDataType,\"1\",\"Fred\",\"East\"",
                ["Value 2"] = "Values,SomeDataType,\"2\",\"Bert\",\"West\"",
            };

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
        }

        [TestMethod]
        public void FullTypedTest()
        {
            // Arrange
            const string metadataPrefix = "Metadata";
            var propertyNames = new[] { "Name", "Number", };
            var options =
                new Options()
                    .ForMetadata<SomeMetadata>(metadataPrefix, propertyNames[0], propertyNames[1])
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
                    .TypedMetadata<Foo, SomeMetadata>(fooMetadata)
                    .TypedData<Foo>(fooData)
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

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
                ["Foo properties"] = "Properties,Foo,Id,Name,Value",
                ["Foo value 1"] = "Values,Foo,1,\"Fred\",\"East\"",
                ["Foo value 2"] = "Values,Foo,2,\"Bert\",\"West\"",
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

            Assert.AreEqual(expectedLineCount, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            Assert.AreEqual(expectedNumberOfCommentLines,
                Regex.Matches(text, "(?:\r?\n){0,2}# ").Count,
                "Unexpected number of comments.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
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
                    .Serialize();

            // Assert
            Assert.IsNotNull(text, "Failed to serialize.");

            if (logSerializedOutput)
                Logger.LogMessage("{0}", text);

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
                ["Foo properties"] = "Properties,Foo,Id,Name,Value",
                ["Foo value 1"] = "Values,Foo,\"1\",\"Fred\",\"East\"",
                ["Foo value 2"] = "Values,Foo,\"2\",\"Bert\",\"West\"",
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

            Assert.AreEqual(expectedLineCount, text.Split(Environment.NewLine).Length,
                "Unexpected number of lines.");

            Assert.AreEqual(expectedNumberOfCommentLines,
                Regex.Matches(text, "(?:\r?\n){0,2}# ").Count,
                "Unexpected number of comments.");

            foreach (var check in checks)
            {
                var pattern = string.Format(PATTERN, check.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline),
                    EXPECTED_MESSAGE, check.Key);
            }
        }

        #region Model

        private class Foo
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public string? Value { get; set; }
        }

        private class SomeMetadata
        {
            public string? Name { get; set; }

            public int Number { get; set; }
        }

        #endregion
    }
}
