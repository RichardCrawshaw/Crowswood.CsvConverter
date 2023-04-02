namespace Crowswood.CsvConverter.Exceptions
{
    public class DataMustBeDeserializedException : InvalidOperationException
    {
        private const string MESSAGE = "The data must be deserialized before being retrieved.";

        public DataMustBeDeserializedException()
            : base(MESSAGE) { }

        public DataMustBeDeserializedException(string message)
            : base($"{MESSAGE} {message}") { }

        public DataMustBeDeserializedException(string message, Exception innerException)
            : base($"{MESSAGE} {message}", innerException) { }
    }
}
