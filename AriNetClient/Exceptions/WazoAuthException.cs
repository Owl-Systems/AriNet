namespace AriNetClient.Exceptions
{
    // Exceptions/WazoAuthException.cs
    public class WazoAuthException : WazoException
    {
        public WazoAuthException(string message) : base(message)
        {
        }

        public WazoAuthException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
