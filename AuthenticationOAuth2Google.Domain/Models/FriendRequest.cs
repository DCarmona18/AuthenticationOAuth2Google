using AuthenticationOAuth2Google.Infrastructure.Enums;

namespace AuthenticationOAuth2Google.Domain.Models
{
    public class FriendRequest
    {
        public string Id { get; set; }
        public string AvatarUrl { get; set; }
        public string Name { get; set; }
        public UserRequestStatus Status { get; set; }
    }
}
