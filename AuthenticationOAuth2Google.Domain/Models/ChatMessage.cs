using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuthenticationOAuth2Google.Domain.Models
{
    public class ChatMessage
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public string Message { get; set; }
        public string Type { get; set; }
        public bool Seen { get; set; }
        public DateTime SentAt { get; set; }
    }
}
