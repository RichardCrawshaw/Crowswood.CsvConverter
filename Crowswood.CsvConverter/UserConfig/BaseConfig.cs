namespace Crowswood.CsvConverter.UserConfig
{
    internal abstract class BaseConfig
    {
        public string Name { get; }
        public string Value { get; }

        protected BaseConfig(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }
    }
}
