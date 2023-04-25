using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Crowswood.CsvConverter.Tests.Internal
{
    [TestClass]
    public class OptionsTests
    {
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
            options.ForMember<OptionsFoo, int>(foo => foo.Id, "ID");
            options.ForMetadata<OptionsFoo>("Foo", "Bar", "Baz");
            options.ForType<OptionsFoo>();

            // Assert
            Assert.IsNotNull(options, "Failed to generate 'Options.None'.");
            Assert.AreEqual(0, options.OptionMembers.Length, "There should be no 'OptionMembers'.");
            Assert.AreEqual(0, options.OptionMetadata.Length, "There should be no 'OptionsMetadata'.");
            Assert.AreEqual(0, options.OptionTypes.Length, "There should be no 'OptionsTypes'.");
        }

        [TestMethod]
        public void StandardUseTest()
        {
            // Arrange
            var options =
                new Options()
                    .ForType<OptionsFoo>()
                    .ForMember<OptionsFoo, int>(foo => foo.Id, "ID")
                    .ForMetadata<OptionsBar>("Foo", "Bar", "Baz");

            // Assert
            Assert.AreEqual(1, options.OptionTypes.Length, "Unexpected number of Types.");
            Assert.AreEqual(typeof(OptionsFoo), options.OptionTypes[0].Type, "Unexpected OptionTypes 0 Type.");

            Assert.AreEqual(1, options.OptionMembers.Length, "Unexpected number of Members.");
            Assert.AreEqual(typeof(OptionsFoo), options.OptionMembers[0].Type, "Unexpected OptionMembers 0 Type.");
            Assert.AreEqual("ID", options.OptionMembers[0].Name, "Unexpected OptionMembers 0 Name.");
            Assert.AreEqual(typeof(OptionsFoo).GetProperty("Id"), options.OptionMembers[0].Property, "Unexpected OptionMembers 0 Property.");

            Assert.AreEqual(1, options.OptionMetadata.Length, "Unexpected number of Metadata.");
            Assert.AreEqual(typeof(OptionsBar), options.OptionMetadata[0].Type, "Unexpected OptionMetadata 0 Type.");
            Assert.AreEqual("Foo", options.OptionMetadata[0].Prefix, "Unexpected OptionMetadata 0 Prefix.");
            Assert.AreEqual(typeof(OptionTypedMetadata<OptionsBar>), options.OptionMetadata[0].GetType(), "Unexpected OptionMetadata 0 generic type.");
            Assert.IsTrue(
                ((OptionTypedMetadata<OptionsBar>)options.OptionMetadata[0]).PropertyNames
                    .GroupBy(propertyName => propertyName)
                    .ToDictionary(n => n.Key, n => n.Count())
                    .All(kvp => (kvp.Key == "Bar" && kvp.Value == 1) ||
                                (kvp.Key == "Baz" && kvp.Value == 1)),
                "Unexpected OptionMetadata 0 PropertyNames.");
        }

        [TestMethod]
        public void DynamicTypeTest()
        {
            // Arrange
            var options =
                new Options()
                    .ForType("Foo", "Id", "Name");

            // Assert
            Assert.AreEqual(1, options.OptionTypes.Length, "Unexpected number of option types.");

            var ot = options.OptionTypes.First();

            Assert.IsInstanceOfType(ot, typeof(OptionDynamicType), "Unexpected option type.");

            var odt = ot as OptionDynamicType;

            Assert.AreEqual("Foo", odt?.Name, "Unexpected option type name.");
            Assert.AreEqual(2, odt?.PropertyNames.Length, "Unexpected number of parameters.");
            Assert.AreEqual("Id", odt?.PropertyNames[0], "Unexpecte name for parameter 0.");
            Assert.AreEqual("Name", odt?.PropertyNames[1], "Unexpecte name for parameter 1.");
        }

        [TestMethod]
        public void RefernecesTest()
        {
            // Arrange
            var options =
                new Options()
                    .ForReferences("AnId", "TheName")
                    .ForReferences("OptionsFoo", "Identity", "FullName")
                    .ForReferences<OptionsBar>("ID", "AnotherName");

            // Assert
            Assert.AreEqual(3, options.OptionsReferences.Length, "Unexpected number of option references.");

            ReferenceTestBody<OptionReference>("global", options, 0, null, "AnId", "TheName");
            ReferenceTestBody<OptionReferenceType>("OptionsFoo", options, 1, "OptionsFoo", "Identity", "FullName");
            ReferenceTestBody<OptionReferenceType<OptionsBar>>("OptionsBar", options, 2, "OptionsBar", "ID", "AnotherName");
        }

        [TestMethod]
        public void SerializationTest()
        {
            // Act
            var options =
                new Options()
                    .ForMetadata<Metadata>("Metadata", "Id", "Name")
                    .ForMetadata("Simple", true, "Id", "Direction")
                    .Serialize(serialize =>
                        serialize
                            .GlobalConfig(Converter.Keys.GlobalConfig.PropertyPrefix, "B")
                            .GlobalConfig(new Dictionary<string, string>
                            {
                                [Converter.Keys.GlobalConfig.ConversionTypePrefix] = "D",
                                [Converter.Keys.GlobalConfig.ConversionValuePrefix] = "F",
                            })
                            .TypeConfig<Foo>(Converter.Keys.TypedConfig.ConversionTypePrefix, "Y")
                            .TypeConfig(typeof(Foo), Converter.Keys.TypedConfig.ConversionValuePrefix, "P")
                            .TypeConfig("Foo", Converter.Keys.TypedConfig.PropertyPrefix, "N")
                            .TypeConfig<Foo>(new Dictionary<string, string>
                            {
                                [Converter.Keys.TypedConfig.ReferenceIdColumnName] = "H",
                            })
                            .TypeConfig(typeof(Foo), new Dictionary<string, string>
                            {
                                [Converter.Keys.TypedConfig.ReferenceNameColumnName] = "J",
                            })
                            .TypeConfig("Foo", new Dictionary<string, string>
                            {
                                [Converter.Keys.TypedConfig.ValuesPrefix] = "S",
                            })
                            .TypeConversion(new Dictionary<string, string>
                            {
                                ["aaa"] = "BBB",
                                ["ccc"] = "DDD",
                            })
                            .ValueConversion(new Dictionary<string, string>
                            {
                                ["ZZZ"] = "yyy",
                                ["XXX"] = "www,"
                            })
                            .TypedMetadata<Metadata, Foo>(new List<Metadata>
                            {
                                new Metadata{ Id = 1, Name = "Fred", },
                            })
                            .TypedMetadata<Metadata, Bar>(new List<Metadata>
                            {
                                new Metadata { Id = 2, Name = "Bert", },
                            })
                            .TypelessMetadata<Foo>("Simple", new Dictionary<string, string>
                            {
                                ["Id"] = 2.ToString(),
                                ["Direction"] = "North",
                            })
                            .TypelessMetadata<Bar>("Simple", new Dictionary<string, string>
                            {
                                ["Id"] = 4.ToString(),
                                ["Direction"] = "South",
                            }))
                    .SetPrefixes("AAA", "BBB");

            // Assert
            Assert.IsNotNull(options.OptionSerialize, "Failed to generate serialization options object.");

            var array = options.OptionSerialize.ToArray();
            Assert.IsNotNull(array, "Failed to retrieve value from serialization options object.");

            const string PATTERN = "^{0}\r?$";
            Dictionary<string, string> expected = new()
            {
                ["GlobalConfig PropertyPrefix"] = "GlobalConfig,PropertyPrefix,B",
                ["GlobalConfig ConversionTypePrefix"] = "GlobalConfig,ConversionTypePrefix,D",
                ["GlobalConfig ConversionValuePrefix"] = "GlobalConfig,ConversionValuePrefix,F",
                ["TypedConfig ConversionTypePrefix"] = "TypedConfig,Foo,ConversionTypePrefix,Y",
                ["TypedConfig ConversionValuePrefix"] = "TypedConfig,Foo,ConversionValuePrefix,P",
                ["TypedConfig PropertyPrefix"] = "TypedConfig,Foo,PropertyPrefix,N",
                ["TypedConfig ReferenceIdColumnName"] = "TypedConfig,Foo,ReferenceIdColumnName,H",
                ["TypedConfig ReferenceNameColumnName"] = "TypedConfig,Foo,ReferenceNameColumnName,J",
                ["TypedConfig ValuesPrefix"] = "TypedConfig,Foo,ValuesPrefix,S",
                ["Type Conversion aaa -> BBB"] = "ConversionTypePrefix,\"aaa\",\"BBB\"",
                ["Type Conversion ccc -> DDD"] = "ConversionTypePrefix,\"ccc\",\"DDD\"",
                ["Value Conversion ZZZ -> yyy"] = "ConversionValuePrefix,\"ZZZ\",\"yyy\"",
                ["Value Conversion XXX -> www"] = "ConversionValuePrefix,\"XXX\",\"www,\"",
                ["Metadata for Foo 1"] = "Metadata,Foo,1,\"Fred\"",
                ["Metadata for Bar 2"] = "Metadata,Bar,2,\"Bert\"",
                ["Simple for Foo 2"] = "Simple,Foo,\"2\",\"North\"",
                ["Simple for Bar 4"] = "Simple,Bar,\"4\",\"South\"",
            };

            Assert.AreEqual(expected.Count, array.Length, "Unexpected number of lines.");

            var text = string.Join(Environment.NewLine, array);

            Logger.LogMessage("{0}", text);

            //GlobalConfig,PropertyPrefix,B
            //GlobalConfig,ConversionTypePrefix,D
            //GlobalConfig,ConversionValuePrefix,F
            //TypedConfig,Foo,ConversionTypePrefix,Y
            //TypedConfig,Foo,ConversionValuePrefix,P
            //TypedConfig,Foo,PropertyPrefix,N
            //TypedConfig,Foo,ReferenceIdColumnName,H
            //TypedConfig,Foo,ReferenceNameColumnName,J
            //TypedConfig,Foo,ValuesPrefix,S
            //ConversionTypePrefix,"aaa","BBB"
            //ConversionTypePrefix,"ccc","DDD"
            //ConversionValuePrefix,"ZZZ","yyy"
            //ConversionValuePrefix,"XXX","www,"
            //Metadata,Foo,1,"Fred"
            //Metadata,Bar,2,"Bert"
            //Simple,Foo,"2","North"
            //Simple,Bar,"4","South"

            const string MESSAGE = "Failed to find '{0}'.";
            foreach (var kvp in expected)
            {
                var pattern = string.Format(PATTERN, kvp.Value);
                Assert.IsTrue(Regex.IsMatch(text, pattern, RegexOptions.Multiline), MESSAGE, kvp.Key);
            }
        }

        #region Support routines

        private static void ReferenceTestBody<T>(string name, Options options, int index, string? typeName, string idProperty, string nameProperty)
            where T : OptionReference
        {
            Assert.IsInstanceOfType(options.OptionsReferences[index], typeof(T),
                "Unexpected option reference: {0}.", name);

            var reference = options.OptionsReferences[index] as T;

            if (reference is OptionReferenceType optionReferenceType && typeName is not null)
                Assert.AreEqual(typeName, optionReferenceType.TypeName,
                    "Unexpected type name: {0}.", name);

            Assert.AreEqual(idProperty, reference?.IdPropertyName,
                "Unexpected Id property name: {0}.", name);
            Assert.AreEqual(nameProperty, reference?.NamePropertyName,
                "Unexpected Name property name: {0}.", name);
        }

        #endregion

        #region Test model

        private class Foo
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        private class Bar
        {
            public int Id { get; set; }
            public string? Value { get; set; }
        }

        private class OptionsFoo
        {
            public int Id { get; set; }
        }

        private class OptionsBar { }

        private class Metadata
        {
            public int Id { get; set; }
            public string? Name { get; set; }
        }

        #endregion
    }
}
