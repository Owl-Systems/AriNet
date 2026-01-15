namespace AriNetClient.Exceptions
{
    public class WazoException : Exception
    {
        public int StatusCode { get; }

        public WazoException(string message) : base(message)
        {
        }

        public WazoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public WazoException(string message, int statusCode)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
