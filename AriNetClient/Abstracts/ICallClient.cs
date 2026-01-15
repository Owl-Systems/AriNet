using AriNetClient.Models.Calls;
using AriNetClient.Models.Common;

namespace AriNetClient.Abstracts
{
    public interface ICallClient
    {
        Task<WazoResponse<Call>> OriginateCallAsync(OriginateRequest request);
        Task<WazoResponse<bool>> HangupCallAsync(string callId);
        Task<WazoResponse<List<Call>>> GetActiveCallsAsync();
        Task<WazoResponse<Call>> GetCallAsync(string callId);
    }
}
