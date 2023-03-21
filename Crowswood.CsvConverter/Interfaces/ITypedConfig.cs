namespace Crowswood.CsvConverter.Interfaces
{
    public interface ITypedConfig
    {
        string Name { get; }

        string TypeName { get; }

        string Value { get; }
    }
}
