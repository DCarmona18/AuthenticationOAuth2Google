using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Infrastructure.Context.Entities
{
    public class ConnectedUserEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserId { get; set; }

        public string ConnectionId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public string UserAgent { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime ConnectedAt { get; set; }
    }
}
