namespace Crowswood.CsvConverter.Model
{
    internal class TypelessNames : Typeless<string[]>
    {
        private readonly List<string> names = new();

        public int Length => names.Count;

        public string this[int index] => names[index];

        public int this[string name] => this.names.IndexOf(name);

        public TypelessNames(string[] names) => this.names.AddRange(names);

        public override string[] Get() => this.names.ToArray();
    }
}
