﻿using MongoDB.Driver;
using System.Linq.Expressions;

namespace AuthenticationOAuth2Google.Infrastructure.Interfaces
{
    public interface IMongoDBRepository<T> where T : class
    {
        Task<List<T>> GetAsync();
        Task<T> GetByIdAsync(string id);
        Task<T> CreateAsync(T data);
        Task AddToCollectionAsync<D>(string id, D dataToAdd, string fieldDefinition);
        Task<UpdateResult> UpdateById(string id, UpdateDefinition<T> updateDefinitionBuilder);
        Task DeleteAsync(string id);
        Task DeleteBulkAsync(Expression<Func<T, bool>> expression);
        IQueryable<T> GetBy(Expression<Func<T, bool>> expression);
        Task<UpdateResult> UpdateBulk(FilterDefinition<T> filter, UpdateDefinition<T> update);
        UpdateDefinitionBuilder<T> BuildUpdateDefinition();
        FilterDefinitionBuilder<T> FilterDefinitionBuilder();
    }
}
