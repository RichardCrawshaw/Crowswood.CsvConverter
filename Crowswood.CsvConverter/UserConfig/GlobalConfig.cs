using Crowswood.CsvConverter.Interfaces;

namespace Crowswood.CsvConverter.UserConfig
{
    internal sealed class GlobalConfig : BaseConfig,
        IGlobalConfig
    {
        public GlobalConfig(string name, string value) : base(name, value) { }
    }
}
