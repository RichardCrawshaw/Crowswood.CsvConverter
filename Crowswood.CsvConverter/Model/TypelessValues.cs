namespace Crowswood.CsvConverter.Model
{
    internal class TypelessValues : Typeless<string[]>
    {
        private readonly List<string> values = new();

        public string this[int index]
        {
            get => values[index];
            set => values[index] = value;
        }

        public TypelessValues(IEnumerable<string> values) => this.values.AddRange(values);

        public override string[] Get() => this.values.ToArray();
    }
}
