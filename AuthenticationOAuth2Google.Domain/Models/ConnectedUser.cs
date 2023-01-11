namespace AuthenticationOAuth2Google.Domain.Models
{
    public class ConnectedUser
    {
        public string ConnectionId { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string UserAgent { get; set; }
        public DateTime ConnectedAt { get; set; }
    }
}
