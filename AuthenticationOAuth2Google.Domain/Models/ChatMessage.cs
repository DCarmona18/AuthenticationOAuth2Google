namespace AuthenticationOAuth2Google.Domain.Models
{
    public class ChatMessage
    {
        public string UserIdSender { get; set; }

        public string Message { get; set; }
        public string AvatarUrl { get; set; }
        public DateTime SentAt { get; set; }
    }
}
