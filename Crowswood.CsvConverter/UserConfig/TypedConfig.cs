using Crowswood.CsvConverter.Interfaces;

namespace Crowswood.CsvConverter.UserConfig
{
    internal sealed class TypedConfig : BaseConfig,
        ITypedConfig
    {
        public string TypeName { get; }

        public TypedConfig(string typeName, string name, string value) 
            : base(name, value) => this.TypeName = typeName;
    }
}
