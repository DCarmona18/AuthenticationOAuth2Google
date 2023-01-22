using AuthenticationOAuth2Google.Domain.Models;

namespace AuthenticationOAuth2Google.Domain.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessage message);
        Task SentMessage(ChatMessage message);
        Task ConnectedToHub(ConnectedUser user);
        Task DisconnectedFromHub(ConnectedUser user);
    }
}
