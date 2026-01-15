using AriNetClient.Abstracts;
using AriNetClient.Configuration;
using AriNetClient.Models.Common;
using AriNetClient.Models.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AriNetClient.Clients
{
    public class UserClient : BaseWazoClient, IUserClient
    {
        public UserClient(
            HttpClient httpClient,
            IOptions<WazoOptions> options,
            ILogger<UserClient> logger)
            : base(httpClient, options, logger)
        {
        }

        public async Task<WazoResponse<PaginatedResponse<User>>> GetUsersAsync(
            int limit = 100,
            int offset = 0,
            string search = null)
        {
            var queryParams = new List<string>
            {
                $"limit={limit}",
                $"offset={offset}"
            };

            if (!string.IsNullOrEmpty(search))
            {
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            }

            var queryString = string.Join("&", queryParams);
            var url = $"/api/confd/{_options.ApiVersion}/users?{queryString}";

            return await ExecuteRequestAsync<PaginatedResponse<User>>(() =>
                _httpClient.GetAsync(url));
        }

        public async Task<WazoResponse<User>> GetUserAsync(string userId)
        {
            return await ExecuteRequestAsync<User>(() =>
                _httpClient.GetAsync($"/api/confd/{_options.ApiVersion}/users/{userId}"));
        }

        public async Task<WazoResponse<User>> CreateUserAsync(User user)
        {
            return await ExecuteRequestAsync<User>(() =>
            {
                var content = CreateJsonContent(user);
                return _httpClient.PostAsync($"/api/confd/{_options.ApiVersion}/users", content);
            });
        }

        public async Task<WazoResponse<bool>> UpdateUserAsync(string userId, User user)
        {
            return await ExecuteRequestAsync<bool>(() =>
            {
                var content = CreateJsonContent(user);
                return _httpClient.PutAsync($"/api/confd/{_options.ApiVersion}/users/{userId}", content);
            });
        }

        public async Task<WazoResponse<bool>> DeleteUserAsync(string userId)
        {
            return await ExecuteRequestAsync<bool>(() =>
                _httpClient.DeleteAsync($"/api/confd/{_options.ApiVersion}/users/{userId}"));
        }
    }
}
