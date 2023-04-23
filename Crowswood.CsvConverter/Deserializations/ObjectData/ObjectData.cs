namespace Crowswood.CsvConverter.Deserializations
{
    internal sealed class ObjectData : BaseObjectData
    {
        /// <summary>
        /// Gets the name of the type of the object.
        /// </summary>
        public override string ObjectTypeName { get; }

        public ObjectData(Deserialization.DeserializationFactory factory, string objectTypeName)
            : base(factory) => this.ObjectTypeName = objectTypeName;
    }
}
