using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Infrastructure.Context.Entities
{
    public class ChatMessageEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }

        public string Message { get; set; }
        public string Type { get; set; }
        public bool Seen { get; set; }
        public DateTime SentAt { get; set; }
    }
}
