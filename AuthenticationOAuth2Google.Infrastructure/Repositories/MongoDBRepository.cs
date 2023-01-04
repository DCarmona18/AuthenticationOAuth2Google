using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using AuthenticationOAuth2Google.Infrastructure.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthenticationOAuth2Google.Infrastructure.Repositories
{
    public class MongoDBRepository : IMongoDBRepository
    {
        private readonly IMongoCollection<Playlist> _playlistCollection;

        public MongoDBRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _playlistCollection = database.GetCollection<Playlist>(mongoDBSettings.Value.CollectionName);
        }

        public async Task AddToPlaylistAsync(string id, string movieId)
        {
            FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
            UpdateDefinition<Playlist> update = Builders<Playlist>.Update.AddToSet<string>("movieIds", movieId);
            await _playlistCollection.UpdateOneAsync(filter, update);
        }

        public async Task CreateAsync(Playlist playlist) 
            => await _playlistCollection.InsertOneAsync(playlist);

        public async Task DeleteAsync(string id)
        {
            FilterDefinition<Playlist> filter = Builders<Playlist>.Filter.Eq("Id", id);
            await _playlistCollection.DeleteOneAsync(filter);
        }

        public async Task<List<Playlist>> GetAsync() =>
            await _playlistCollection.Find(new BsonDocument()).ToListAsync();
    }
}
