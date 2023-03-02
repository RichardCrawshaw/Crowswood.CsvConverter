using Crowswood.CsvConverter.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Crowswood.CsvConverter.Tests
{
    [TestClass]
    public class UserConfigTests
    {
        [TestMethod]
        public void DefaultConstructorTest()
        {
            // Assign


            // Act
            var handler = new ConfigHandler();

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
            // Assign
            var text = @"
GlobalConfig,ExampleName,ExampleValue
TypedConfig,TypeName,ExampleName,ExampleValue2
";
            var lines =
                text.Split("\r\n", 
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries);

            // Act
            ConfigHandler? handler = null;

            Logger.LogMessage("Text = {0}", text);
            for (var i = 0; i < lines.Length; i++)
                Logger.LogMessage("Line {0}: '{1}'", i, lines[i]);

            try
            {
                handler = new ConfigHandler(Options.None, lines);
            }
            catch (Exception ex)
            {
                if (ex.Data.Count > 0)
                {
                    foreach(var key in ex.Data.Keys)
                    {
                        Logger.LogMessage("{0}", key);
                        var value = ex.Data[key];
                        if (value is List<string> list)
                            foreach (var item in list)
                                Logger.LogMessage("    {0}", item);
                    }
                }
                Assert.Fail(ex.Message);
            }

            // Assert
            Assert.IsNotNull(handler, "Failed to create handler.");

            Assert.AreEqual(1, handler.GlobalConfig.Length, "Unexpected number of global config items.");
            Assert.AreEqual(1, handler.TypedConfig.Length, "Unexpected number of typed config items.");

            var globalConfig = handler.GetGlobal(name: "ExampleName");
            Assert.IsNotNull(globalConfig, "Failed to find expected global config item.");
            Assert.AreEqual("ExampleValue", globalConfig.Value, "Unexpected global config value.");

            var typedConfigs = handler.GetTyped(name: "ExampleName");
            Assert.IsNotNull(typedConfigs, "Failed to find expected typed config items.");
            Assert.AreEqual(1, typedConfigs.Count(), "Unexpected number of typed config items retrieved.");

            var typedConfig1 = typedConfigs.FirstOrDefault();
            Assert.IsNotNull(typedConfig1, "Failed to find expected typed config item.");
            Assert.AreEqual("ExampleValue2", typedConfig1.Value, "Unexpected typed config value (method 1).");

            var typedConfig2 = handler.GetTyped(typeName: "TypeName", name: "ExampleName");
            Assert.IsNotNull(typedConfig2, "Failed to get expected typed config item for type.");
            Assert.AreEqual("ExampleValue2", typedConfig2.Value, "Unexpected typed config value (method 2).");
        }
    }
}
