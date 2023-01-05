using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace AuthenticationOAuth2Google.Infrastructure.Context.Entities
{
    public class Playlist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string username { get; set; } = null!;

        [BsonElement("items")]
        [JsonPropertyName("moviesIds")]
        public List<string> movieIds { get; set; } = null!;
    }
}
