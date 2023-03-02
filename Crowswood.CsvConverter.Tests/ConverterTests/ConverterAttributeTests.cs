using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Crowswood.CsvConverter.Tests.ConverterTests.ConverterBaseTests;

namespace Crowswood.CsvConverter.Tests.ConverterTests
{
    [TestClass]
    public class ConverterAttributeTests
    {
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

        #region Model classes

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

        #endregion
    }
}
