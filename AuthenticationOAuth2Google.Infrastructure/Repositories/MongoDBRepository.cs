﻿using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using AuthenticationOAuth2Google.Infrastructure.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using static MongoDB.Driver.WriteConcern;

namespace AuthenticationOAuth2Google.Infrastructure.Repositories
{
    public class MongoDBRepository<T> : IMongoDBRepository<T> where T : class
    {
        public readonly IMongoCollection<T> _genericCollection;

        public MongoDBRepository(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            _genericCollection = database.GetCollection<T>(typeof(T).Name);
        }

        public async Task AddToCollectionAsync<D>(string id, D dataToAdd, string fieldDefinition)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("Id", id);
            UpdateDefinition<T> update = Builders<T>.Update.AddToSet(fieldDefinition, dataToAdd);
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

        public async Task DeleteBulkAsync(Expression<Func<T, bool>> expression) 
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Where(expression);
            await _genericCollection.DeleteManyAsync(filter);
        }

        public async Task<List<T>> GetAsync() =>
            await _genericCollection.Find(new BsonDocument()).ToListAsync();

        public async Task<T> GetByIdAsync(string id)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("Id", id);
            return await (await _genericCollection.FindAsync(filter)).FirstOrDefaultAsync();
        }

        public IQueryable<T> GetBy(Expression<Func<T, bool>> expression) 
        {
            return _genericCollection.AsQueryable().Where(expression);
        }

        public async Task<UpdateResult> UpdateById(string id, UpdateDefinition<T> updateDefinitionBuilder)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq("Id", id);
            UpdateDefinition<T> update = Builders<T>.Update.Set("algo", 1);
            return await _genericCollection.UpdateOneAsync(filter, updateDefinitionBuilder);
        }

        public async Task<UpdateResult> UpdateBulk(FilterDefinition<T> filter, UpdateDefinition<T> update) 
        {
            return await _genericCollection.UpdateManyAsync(filter, update);
        }

        public UpdateDefinitionBuilder<T> BuildUpdateDefinition() 
        {
            return Builders<T>.Update;
        }

        public FilterDefinitionBuilder<T> FilterDefinitionBuilder()
        {
            return Builders<T>.Filter;
        }
    }
}
