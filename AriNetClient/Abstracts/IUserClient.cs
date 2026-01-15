using AriNetClient.Models.Common;
using AriNetClient.Models.Users;

namespace AriNetClient.Abstracts
{
    public interface IUserClient
    {
        Task<WazoResponse<PaginatedResponse<User>>> GetUsersAsync(
            int limit = 100,
            int offset = 0,
            string search = null);

        Task<WazoResponse<User>> GetUserAsync(string userId);
        Task<WazoResponse<User>> CreateUserAsync(User user);
        Task<WazoResponse<bool>> UpdateUserAsync(string userId, User user);
        Task<WazoResponse<bool>> DeleteUserAsync(string userId);
    }
}
