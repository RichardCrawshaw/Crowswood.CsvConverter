using Crowswood.CsvConverter.Extensions;
using Crowswood.CsvConverter.Handlers;

namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class UserConfigTests
    {
        [TestMethod]
        public void DefaultConstructorTest()
        {
            // Arrange
            var options = Options.None;

            // Act
            var handler = new ConfigHandler(options);

            // Assert
            Assert.IsNotNull(handler, "Failed to create handler.");

            Assert.AreEqual(0, handler.GlobalConfig.Length, 
                "Default constructor failed to provide empty GlobalConfig.");
            Assert.AreEqual(0, handler.TypedConfig.Length, 
                "Default constructor failed to provide empty TypedConfig.");
        }

        [TestMethod]
        public void StandardConstructorTest()
        {
            // Arrange
            var text = @"
GlobalConfig,ExampleName,ExampleValue
TypedConfig,TypeName,ExampleName,ExampleValue2
";
            var lines =
                text.Split("\r\n".ToCharArray(), 
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            // Act
            var handler = new ConfigHandler(Options.None, lines);

            // Assert
            Assert.IsNotNull(handler, "Failed to create handler.");

            Assert.AreEqual(1, handler.GlobalConfig.Length, "Unexpected number of global config items.");
            Assert.AreEqual(1, handler.TypedConfig.Length, "Unexpected number of typed config items.");

            var globalConfig = handler.GlobalConfig.GetGlobal(name: "ExampleName");
            Assert.IsNotNull(globalConfig, "Failed to find expected global config item.");
            Assert.AreEqual("ExampleValue", globalConfig.Value, "Unexpected global config value.");

            var typedConfigs = handler.TypedConfig.GetTyped(name: "ExampleName");
            Assert.IsNotNull(typedConfigs, "Failed to find expected typed config items.");
            Assert.AreEqual(1, typedConfigs.Count(), "Unexpected number of typed config items retrieved.");

            var typedConfig1 = typedConfigs.FirstOrDefault();
            Assert.IsNotNull(typedConfig1, "Failed to find expected typed config item.");
            Assert.AreEqual("ExampleValue2", typedConfig1.Value, "Unexpected typed config value (method 1).");

            var typedConfig2 = handler.TypedConfig.GetTyped(typeName: "TypeName", name: "ExampleName");
            Assert.IsNotNull(typedConfig2, "Failed to get expected typed config item for type.");
            Assert.AreEqual("ExampleValue2", typedConfig2.Value, "Unexpected typed config value (method 2).");
        }
    }
}
