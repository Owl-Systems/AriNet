using AriNetClient.Models.Auth;
using AriNetClient.Models.Common;

namespace AriNetClient.Abstracts
{
    public interface IAuthClient
    {
        Task<WazoResponse<TokenResponse>> GetTokenAsync(
            string username,
            string password,
            int? expiration = null);

        Task<WazoResponse<bool>> ValidateTokenAsync(string token);
        Task<WazoResponse<bool>> RevokeTokenAsync(string token);
    }
}
