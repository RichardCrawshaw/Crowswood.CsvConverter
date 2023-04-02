namespace Crowswood.CsvConverter.Exceptions
{
    public class NoObjectDataException : InvalidOperationException
    {
        private const string MESSAGE = "There is no object data for type '{0}'.";

        public NoObjectDataException(Type type)
            : base(string.Format(MESSAGE, type.Name)) { }

        public NoObjectDataException(Type type, string message)
            : base($"{string.Format(MESSAGE, type.Name)} {message}") { }

        public NoObjectDataException(Type type, Exception innerException) 
            : base(string.Format(MESSAGE, type.Name), innerException) { }

        public NoObjectDataException(Type type, string message, Exception innerException)
            : base($"{string.Format(MESSAGE, type.Name)} {message}", innerException) { }

        public NoObjectDataException(string typeName, string? message)
            : base($"{string.Format(MESSAGE, typeName)} {message}".Trim()) { }
    }
}
