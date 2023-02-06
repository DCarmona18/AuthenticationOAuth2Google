using AuthenticationOAuth2Google.Domain.Models;

namespace AuthenticationOAuth2Google.Domain.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessage message);
        Task NewMessage(ChatMessage message);
        Task ConnectedToHub(ConnectedUser user);
        Task DisconnectedFromHub(ConnectedUser user);
        Task NewFriendRequest(FriendRequest friendRequest);
        Task FriendRequestAccepted(Friend friend);
        Task MarkAsSeen(string userId);
    }
}
