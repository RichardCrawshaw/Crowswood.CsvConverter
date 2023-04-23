namespace Crowswood.CsvConverter.Exceptions
{
    /// <summary>
    /// An exception that is thrown when an attempt is made to access metadata where it has not 
    /// yet been deserialized. It extends <see cref="InvalidOperationException"/>.
    /// </summary>
    public class MetadataNotDeserializedException : InvalidOperationException
    {
        private const string MESSAGE = "The metadata has not been deserialized.";

        public MetadataNotDeserializedException()
            : base(MESSAGE) { }

        public MetadataNotDeserializedException(string message)
            : base($"{MESSAGE} {message}") { }

        public MetadataNotDeserializedException(string message, Exception innerException)
            : base($"{MESSAGE} {message}", innerException) { }
    }
}
