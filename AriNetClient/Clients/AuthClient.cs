using AriNetClient.Abstracts;
using AriNetClient.Configuration;
using AriNetClient.Models.Auth;
using AriNetClient.Models.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AriNetClient.Clients
{
    public class AuthClient : BaseWazoClient, IAuthClient
    {
        public AuthClient(
            HttpClient httpClient,
            IOptions<WazoOptions> options,
            ILogger<AuthClient> logger)
            : base(httpClient, options, logger)
        {
        }

        public async Task<WazoResponse<TokenResponse>> GetTokenAsync(
            string username,
            string password,
            int? expiration = null)
        {
            var request = new TokenRequest
            {
                Username = username ?? _options.Username,
                Password = password ?? _options.Password,
                Expiration = expiration ?? 3600
            };

            var response = await ExecuteRequestAsync<TokenResponse>(() =>
            {
                var content = CreateJsonContent(request);
                return _httpClient.PostAsync("/api/auth/0.1/token", content);
            });

            if (response.Success)
            {
                SetAuthToken(response.Data.Token);
            }

            return response;
        }

        public async Task<WazoResponse<bool>> ValidateTokenAsync(string token)
        {
            var tempToken = _authToken;
            SetAuthToken(token);

            var response = await ExecuteRequestAsync<object>(() =>
                _httpClient.GetAsync("/api/auth/0.1/token"));

            SetAuthToken(tempToken);

            return new WazoResponse<bool>
            {
                Success = response.Success,
                Data = response.Success,
                Error = response.Error,
                StatusCode = response.StatusCode
            };
        }

        public async Task<WazoResponse<bool>> RevokeTokenAsync(string token)
        {
            var response = await ExecuteRequestAsync<object>(() =>
                _httpClient.DeleteAsync($"/api/auth/0.1/token/{token}"));

            return new WazoResponse<bool>
            {
                Success = response.Success,
                Data = response.Success,
                Error = response.Error,
                StatusCode = response.StatusCode
            };
        }
    }
}
