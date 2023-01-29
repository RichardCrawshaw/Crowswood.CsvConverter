namespace Crowswood.CsvConverter
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CsvConverterClassAttribute : Attribute
    {
        public string Name { get; }

        public CsvConverterClassAttribute(string name)
        {
            this.Name = name;
        }
    }
}
