using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using AuthenticationOAuth2Google.Infrastructure.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthenticationOAuth2Google.Infrastructure.Repositories
{
    public class MongoDBRepository<T> : IMongoDBRepository<T> where T : class
    {
        private readonly IMongoCollection<T> _genericCollection;

        public MongoDBRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _genericCollection = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task AddToCollectionAsync<D>(string id, D dataToAdd)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("Id", id);
            UpdateDefinition<T> update = Builders<T>.Update.AddToSet<D>("movieIds", dataToAdd);
            await _genericCollection.UpdateOneAsync(filter, update);
        }

        public async Task<T> CreateAsync(T document)
        {
            await _genericCollection.InsertOneAsync(document);
            return document;
        }

        public async Task DeleteAsync(string id)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("Id", id);
            await _genericCollection.DeleteOneAsync(filter);
        }

        public async Task<List<T>> GetAsync() =>
            await _genericCollection.Find(new BsonDocument()).ToListAsync();

    }
}
