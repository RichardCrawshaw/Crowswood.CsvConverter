namespace Crowswood.CsvConverter.Serializations
{
    /// <summary>
    /// The abstract base class for holding data to be serialized.
    /// </summary>
    public abstract class BaseSerializationData
    {
        /// <summary>
        /// Serialize the data held in the current instance.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the result of the serialization.</returns>
        public abstract string[] Serialize();
    }
}
