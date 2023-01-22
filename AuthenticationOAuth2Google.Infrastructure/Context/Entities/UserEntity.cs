using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace AuthenticationOAuth2Google.Infrastructure.Context.Entities
{
    public enum AUTH_TYPE { 
        GOOGLE,
        USERNAME_PASSWORD
    };

    public class UserEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonIgnoreIfNull]
        public string? OAuthId { get; set; }
        
        public string Email { get; set; }

        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string AvatarUrl { get; set; }
        public string Role { get; set; }
        public bool Enabled { get; set; }
        public DateTime CreationDate { get; set; }
        public AUTH_TYPE AuthType { get; set; }

        [BsonElement("Friends")]
        public List<FriendEntity> Friends { get; set; } = new List<FriendEntity>();
    }
}
