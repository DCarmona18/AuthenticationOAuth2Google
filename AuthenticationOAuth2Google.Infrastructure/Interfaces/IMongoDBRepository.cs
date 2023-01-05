using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Infrastructure.Interfaces
{
    public interface IMongoDBRepository<T> where T : class
    {
        Task<List<T>> GetAsync();
        Task<T> CreateAsync(T data);
        Task AddToCollectionAsync<D>(string id, D dataToAdd);
        Task DeleteAsync(string id);
    }
}
