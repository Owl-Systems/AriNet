using AriNetClient.Abstracts;
using AriNetClient.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AriNetClient.Clients
{
    public class WazoNetClient : IWazoNetClient
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<WazoOptions> _options;
        private bool _initialized = false;

        public IAuthClient Auth { get; private set; }
        public IUserClient Users { get; private set; }
        public ICallClient Calls { get; private set; }
        //public IWebSocketClient WebSocket { get; private set; }

        public WazoNetClient(IServiceProvider serviceProvider, IOptions<WazoOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public async Task InitializeAsync(string? baseUrl = null, string? username = null, string? password = null)
        {
            if (_initialized) return;

            // تحديث الإعدادات إذا تم توفيرها
            if (!string.IsNullOrEmpty(baseUrl))
                _options.Value.BaseUrl = baseUrl;
            if (!string.IsNullOrEmpty(username))
                _options.Value.Username = username;
            if (!string.IsNullOrEmpty(password))
                _options.Value.Password = password;

            // تهيئة العملاء
            Auth = _serviceProvider.GetRequiredService<IAuthClient>();
            Users = _serviceProvider.GetRequiredService<IUserClient>();
            Calls = _serviceProvider.GetRequiredService<ICallClient>();

            // الحصول على التوكن
            var tokenResponse = await Auth.GetTokenAsync(_options.Value.Username, _options.Value.Password);

            if (!tokenResponse.Success)
                throw new Exception($"Failed to authenticate: {tokenResponse.Error}");

            //// تهيئة WebSocket إذا كان مفعلاً
            //if (_options.Value.AutoConnectWebSocket && !string.IsNullOrEmpty(_options.Value.WebSocketUrl))
            //{
            //    WebSocket = new WebSocketClient(
            //        _options.Value.WebSocketUrl,
            //        tokenResponse.Data.Token);

            //    await WebSocket.ConnectAsync();
            //}

            _initialized = true;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var response = await Users.GetUsersAsync(limit: 1);
                return response.Success;
            }
            catch
            {
                return false;
            }
        }

    }
}
