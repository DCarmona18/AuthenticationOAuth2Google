using AuthenticationOAuth2Google.Domain.Models;

namespace AuthenticationOAuth2Google.Domain.Interfaces
{
    public interface IFriendService
    {
        Task<FriendRequest> AddFriendRequest(Friend friend);
        Task<List<Friend>> GetFriendsAndUnseenMessages();

        Task<List<FriendRequest>> GetFriendRequests();

        Task<Friend> AcceptFriendRequest(FriendRequest friendRequest);
    }
}
