using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationOAuth2Google.Infrastructure.Interfaces
{
    public interface IMongoDBRepository
    {
        Task<List<Playlist>> GetAsync();
        Task CreateAsync(Playlist playlist);
        Task AddToPlaylistAsync(string id, string movieId);
        Task DeleteAsync(string id);
    }
}
