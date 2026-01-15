using AriNetClient.Configuration;
using AriNetClient.Models.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AriNetClient.Clients
{
    public abstract class BaseWazoClient
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;
        protected readonly WazoOptions _options;
        protected string _authToken;

        public BaseWazoClient(HttpClient httpClient, IOptions<WazoOptions> options, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _options = options.Value;

            ConfigureHttpClient();
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrEmpty(_authToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authToken);
                _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", _authToken);
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
            ConfigureHttpClient();
        }

        protected async Task<WazoResponse<T>> ExecuteRequestAsync<T>(
            Func<Task<HttpResponseMessage>> requestFunc,
            CancellationToken cancellationToken = default)
        {
            int retryCount = 0;

            while (retryCount <= _options.RetryCount)
            {
                try
                {
                    var response = await requestFunc();

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var data = JsonConvert.DeserializeObject<T>(content);

                        return new WazoResponse<T>
                        {
                            Success = true,
                            Data = data,
                            StatusCode = (int)response.StatusCode
                        };
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger?.LogWarning("Authentication failed, token may have expired");
                        return new WazoResponse<T>
                        {
                            Success = false,
                            Error = "Authentication failed",
                            StatusCode = (int)response.StatusCode
                        };
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger?.LogError("API Error: {StatusCode} - {Content}",
                            response.StatusCode, errorContent);

                        return new WazoResponse<T>
                        {
                            Success = false,
                            Error = errorContent,
                            StatusCode = (int)response.StatusCode
                        };
                    }
                }
                catch (HttpRequestException ex)
                {
                    retryCount++;
                    _logger?.LogError(ex, "HTTP request failed (attempt {RetryCount}/{MaxRetries})",
                        retryCount, _options.RetryCount);

                    if (retryCount > _options.RetryCount)
                    {
                        return new WazoResponse<T>
                        {
                            Success = false,
                            Error = ex.Message,
                            StatusCode = 0
                        };
                    }

                    await Task.Delay(1000 * retryCount, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Unexpected error");
                    return new WazoResponse<T>
                    {
                        Success = false,
                        Error = ex.Message,
                        StatusCode = 0
                    };
                }
            }

            return new WazoResponse<T>
            {
                Success = false,
                Error = "Max retries exceeded",
                StatusCode = 0
            };
        }

        protected StringContent CreateJsonContent(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }
    }
}
