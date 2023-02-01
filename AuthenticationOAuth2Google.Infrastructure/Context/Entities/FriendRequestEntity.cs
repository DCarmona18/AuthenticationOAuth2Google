using AuthenticationOAuth2Google.Infrastructure.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AuthenticationOAuth2Google.Infrastructure.Context.Entities
{
    public class FriendRequestEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        [BsonRepresentation(BsonType.String)]
        public UserRequestStatus Status { get; set; }
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; }
    }
}
