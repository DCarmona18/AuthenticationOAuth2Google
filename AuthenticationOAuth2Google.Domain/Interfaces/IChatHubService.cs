using AuthenticationOAuth2Google.Domain.Models;

namespace AuthenticationOAuth2Google.Domain.Interfaces
{
    public interface IChatHubService
    {
        Task<IEnumerable<ConnectedUser>> GetConnectedUsers();
    }
}
