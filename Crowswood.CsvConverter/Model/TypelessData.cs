namespace Crowswood.CsvConverter.Model
{
    internal class TypelessData : Typeless<(string[], IEnumerable<string[]>)>
    {
        private readonly List<TypelessValues> values = new();

        public TypelessNames Names { get; }

        public IEnumerable<TypelessValues> Values => this.values;

        public string this[int index, string field]
        {
            get => this.values[index][this.Names[field]];
            set => this.values[index][this.Names[field]] = value;
        }

        public string this[string field, int index]
        {
            get => this[index, field];
            set => this[index, field] = value;
        }

        public TypelessData(string[] names) => this.Names = new TypelessNames(names);

        public TypelessData(string[] names, IEnumerable<string[]> values)
            : this(names)
        {
            if (values.Any(v => v.Length != names.Length))
                throw new ArgumentException(
                    "All values must have the same number of items as the names.",
                    nameof(values));

            this.values.AddRange(
                values
                    .Select(v => new TypelessValues(v)));
        }

        public void Add(string[] values)
        {
            if (values.Length != this.Names.Length)
                throw new ArgumentException("The number of values must match the number of names.",
                    nameof(values));
            this.values.Add(new TypelessValues(values));
        }

        public override (string[], IEnumerable<string[]>) Get() =>
            (this.Names.Get(), this.Values.Select(v => v.Get().ToArray()));
    }
}
