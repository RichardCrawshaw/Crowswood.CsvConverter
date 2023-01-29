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
            var objects = converter.Deserialize<object>("some text");

            // Assert
            Assert.AreEqual(0, objects.Count(), "Unexpected number of objects.");
        }

        [TestMethod]
        public void SimpleDeserializeTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Fred""";
            var options =
                new Options()
                    .ForType(new OptionType<Foo>());
            var converter = new Converter(options);

            // Act
            var foos = converter.Deserialize<Foo>(text);

            // Assert
            Assert.AreEqual(1, foos.Count(), "Unexpected number of Foo objects.");

            Assert.AreEqual(1, foos.First().Id, "Incorrect Id.");
            Assert.AreEqual("Fred", foos.First().Name, "Incorrect Name.");
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
                    .ForType(new OptionType<Foo>());
            var converter = new Converter(options);

            // Act
            var foos = converter.Deserialize<Foo>(text);

            // Assert
            Assert.AreEqual(1, foos.Count(), "Unexpected number of Foo objects.");
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
                    .ForType(new OptionType<Bar>());
            var converter = new Converter(options);

            // Act
            var bars = converter.Deserialize<Bar>(text);

            // Assert
            Assert.AreEqual(2, bars.Count(), "Unexpected number of Bar objects.");

            Assert.AreEqual(1, bars.First().Id, "Incorrect Id for 0.");
            Assert.AreEqual("Fred", bars.First().Name, "Incorrect Name for 0.");
            Assert.AreEqual(true, bars.First().Flag, "Incorrect Flag for 0.");
            Assert.AreEqual(99.9d, bars.First().Value, "Incorrect Value for 0.");
            Assert.AreEqual(88.88m, bars.First().Number, "Incorrect Number for 0.");

            Assert.AreEqual(2, bars.Skip(1).First().Id, "Incorrect Id for 1.");
            Assert.AreEqual("Bert", bars.Skip(1).First().Name, "Incorrect Name for 1.");
            Assert.AreEqual(false, bars.Skip(1).First().Flag, "Incorrect Flag for 1.");
            Assert.AreEqual(77.5d, bars.Skip(1).First().Value, "Incorrect Value for 1.");
            Assert.AreEqual(-321.4m, bars.Skip(1).First().Number, "Incorrect Number for 1.");
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
                    .ForType(new OptionType<Bar>())
                    .ForType(new OptionType<Baz>());
            var converter = new Converter(options);

            // Act
            var foos = converter.Deserialize<Foo>(text);
            var bars = foos.Where(foo => foo is Bar);
            var bazs = foos.Where(foo => foo is Baz);

            var bars2 = converter.Deserialize<Bar>(text);
            var bazs2 = converter.Deserialize<Baz>(text);

            // Assert
            Assert.AreEqual(4, foos.Count(), "Unexpected number of Foo objects.");
            Assert.AreEqual(2, bars.Count(), "Unexpected number of Bar objects.");
            Assert.AreEqual(2, bazs.Count(), "Unexpected number of Baz objects.");

            Assert.AreEqual(2, bars2.Count(), "Unexpected number of Bar 2 objects.");
            Assert.AreEqual(2, bazs2.Count(), "Unexpected number of Baz 2 objects.");

            Assert.AreEqual(99, bazs2.First().Id, "Incorrect Id for Baz 0.");
            Assert.AreEqual("Ninty-nine", bazs2.First().Name, "Incorrect Name for Baz 0.");
            Assert.AreEqual("nn", bazs2.First().Code, "Incorrect Code for Baz 0.");
            Assert.AreEqual("This value is 99", bazs2.First().Description, "Incorrect Description for Baz 0.");

            Assert.AreEqual(108, bazs2.Skip(1).First().Id, "Incorrect Id for Baz 1.");
            Assert.AreEqual("One-zero-eight", bazs2.Skip(1).First().Name, "Incorrect Name for Baz 1.");
            Assert.AreEqual("oze", bazs2.Skip(1).First().Code, "Incorrect Code for Baz 1.");
            Assert.AreEqual("Value over a hundred", bazs2.Skip(1).First().Description, "Incorrect Description for Baz 1.");
        }

        [TestMethod]
        public void SimpleSerializeTest()
        {
            // Arrange
            var foos =
                new List<Foo>()
                {
                    new Foo() { Id = 1, Name = "Foo 1", },
                    //new Bar() { Id = 2, Name = "Bar 2", Flag = true, Value = 888.08d, Number = 9966.3m, },
                    //new Baz() { Id = 3, Name = "Baz 3", Code = "iqzy", Description = "This is  some text", },
                };

            var options =
                new Options()
                    .ForType(new OptionType<Foo>());
                    //.ForType(new OptionType<Bar>())
                    //.ForType(new OptionType<Baz>());
            var converter = new Converter(options);

            // Act
            var text = converter.Serialize(foos);

            // Assert
            Assert.IsNotNull(text, "Failed to serialize List of Foo.");
            Assert.IsTrue(text.Contains("Properties,Foo,Id,Name"), "Serialized data does not contain Foo properties line.");
            Assert.IsTrue(text.Contains("Values,Foo,1,\"Foo 1\""), "Serialized data does not contain Foo values line 0.");
        }

        [TestMethod]
        public void MultipleSerializeTest()
        {
            // Arrange
            var foos =
                new List<Foo>()
                {
                    new Foo() { Id = 1, Name = "Foo 1", },
                    new Bar() { Id = 2, Name = "Bar 2", Flag = true, Value = 888.08d, Number = 9966.3m, },
                    new Baz() { Id = 3, Name = "Baz 3", Code = "iqzy", Description = "This is some text", },
                };

            var options =
                new Options()
                    .ForType(new OptionType<Foo>())
                    .ForType(new OptionType<Bar>())
                    .ForType(new OptionType<Baz>());
            var converter = new Converter(options);

            // Act
            var text = converter.Serialize(foos);

            // Assert
            Assert.IsNotNull(text, "Failed to serialize List of Foo.");
            Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger.LogMessage(text);

            Assert.IsTrue(text.Contains("Properties,Foo,Id,Name"), "Serialized data does not contain Foo properties line.");
            Assert.AreEqual(1, text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Count(n => n == "Properties,Foo,Id,Name"), "Extra Foo properties lines.");
            Assert.IsTrue(text.Contains("Values,Foo,1,\"Foo 1\""), "Serialized data does not contain Foo values line 0.");

            Assert.IsTrue(text.Contains("Properties,Bar,Id,Name,Flag,Value,Number"), "Serialized data does not contain Bar properties line.");
            Assert.AreEqual(1, text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Count(n => n == "Properties,Bar,Id,Name,Flag,Value,Number"), "Extra Bar properties lines.");
            Assert.IsTrue(text.Contains("Values,Bar,2,\"Bar 2\",True,888.08,9966.3"), "Serialized data does not contain Bar values line 0.");

            Assert.IsTrue(text.Contains("Properties,Baz,Id,Name,Code,Description"), "Serialized data does not contain Baz properties line.");
            Assert.AreEqual(1, text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Count(n => n == "Properties,Baz,Id,Name,Code,Description"), "Extra Baz properties lines.");
            Assert.IsTrue(text.Contains("Values,Baz,3,\"Baz 3\",\"iqzy\",\"This is some text\""), "Serialized data does not contain Baz values line 0.");
        }

        [TestMethod]
        public void AttributeDeserializeTest()
        {
            // Arrange
            var text = @"
Properties,Foo,Id,Name
Values,Foo,1,""Picture""";
            var converter = new Converter(Options.None);

            // Act
            var data = converter.Deserialize<AttrFoo>(text);

            // Assert
            Assert.IsNotNull(data, "Failed to deserialize into objects with attributes.");

            Assert.AreEqual(1, data.Count(), "Incorrect number of data items with attributes.");

            Assert.AreEqual(1, data.First().Identity, "Incorrect value for Identity of object 1.");
            Assert.AreEqual("Picture", data.First().FullName, "Incorrect value for FullName of object 1.");
        }

        [TestMethod]
        public void AttributeSerializeTest()
        {
            // Arrange
            var data =
                new List<AttrFoo>
                {
                    new AttrFoo { Identity = 1, FullName = "Picture", },
                };

            var converter = new Converter(Options.None);

            // Act
            var text = converter.Serialize(data);

            // Assert
            Assert.IsNotNull(text, "Converter generated null.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(text), "Converter generated nothing.");

            Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger.LogMessage(text);

            Assert.IsTrue(text.Contains("Properties,Foo,Id,Name"), "No properties line generated for Foo.");
            Assert.IsTrue(text.Contains("Values,Foo,1,\"Picture\""), "No values line 0 generated for Foo.");
        }

        #region Model classes

        // WARNING: Don't change the names of the Foo, Bar and Baz classes, their names are tied
        // to the textual data in some of the tests and those tests will then fail.

        private class Foo
        {
            public int Id { get; set; }
            public string? Name { get; set; }
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

        [CsvConverterClass("Foo")]
        private class AttrFoo
        {
            [CsvConverterProperty("Id")]
            public int Identity { get; set; }

            [CsvConverterProperty("Name")]
            public string? FullName { get; set; }
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