namespace AriNetClient.Abstracts
{
    public interface IWazoNetClient
    {
        IAuthClient Auth { get; }
        IUserClient Users { get; }
        ICallClient Calls { get; }
        //IWebSocketClient WebSocket { get; }

        Task InitializeAsync(string baseUrl = null, string username = null, string password = null);
        Task<bool> TestConnectionAsync();
    }
}
