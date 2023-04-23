namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class ObjectData : BaseObjectData
    {
        public override string ObjectTypeName { get; }

        public ObjectData(Deserialization.DeserializationFactory factory, string objectTypeName)
            : base(factory) => this.ObjectTypeName = objectTypeName;
    }
}
