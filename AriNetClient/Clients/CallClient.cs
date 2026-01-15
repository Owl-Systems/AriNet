using AriNetClient.Abstracts;
using AriNetClient.Configuration;
using AriNetClient.Models.Calls;
using AriNetClient.Models.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AriNetClient.Clients
{
    public class CallClient : BaseWazoClient, ICallClient
    {
        public CallClient(HttpClient httpClient, IOptions<WazoOptions> options, ILogger<CallClient> logger)
            : base(httpClient, options, logger) { }

        public async Task<WazoResponse<Call>> OriginateCallAsync(OriginateRequest request)
        {
            return await ExecuteRequestAsync<Call>(() =>
            {
                var content = CreateJsonContent(request);
                return _httpClient.PostAsync($"/api/ari/{_options.ApiVersion}/channels", content);
            });
        }

        public async Task<WazoResponse<bool>> HangupCallAsync(string callId)
        {
            return await ExecuteRequestAsync<bool>(() =>
                _httpClient.DeleteAsync($"/api/ari/{_options.ApiVersion}/channels/{callId}"));
        }

        public async Task<WazoResponse<List<Call>>> GetActiveCallsAsync()
        {
            return await ExecuteRequestAsync<List<Call>>(() =>
                _httpClient.GetAsync($"/api/ari/{_options.ApiVersion}/channels"));
        }

        public async Task<WazoResponse<Call>> GetCallAsync(string callId)
        {
            return await ExecuteRequestAsync<Call>(() =>
                _httpClient.GetAsync($"/api/ari/{_options.ApiVersion}/channels/{callId}"));
        }
    }
}
