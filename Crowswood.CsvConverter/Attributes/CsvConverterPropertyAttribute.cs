namespace Crowswood.CsvConverter
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class CsvConverterPropertyAttribute : Attribute
    {
        public string Name { get; }

        public CsvConverterPropertyAttribute(string name)
        {
            this.Name = name;
        }
    }
}
