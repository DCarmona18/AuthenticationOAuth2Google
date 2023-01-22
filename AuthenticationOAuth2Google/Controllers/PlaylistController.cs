using AuthenticationOAuth2Google.Infrastructure.Context.Entities;
using AuthenticationOAuth2Google.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationOAuth2Google.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly IMongoDBRepository<Playlist> _mongoDBRepository;

        public PlaylistController(IMongoDBRepository<Playlist> mongoDBRepository)
        {
            _mongoDBRepository = mongoDBRepository;
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Playlist playlist)
        {
            await _mongoDBRepository.CreateAsync(playlist);
            return CreatedAtAction(nameof(Get), new { id = playlist.Id }, playlist);
        }

        [HttpGet]
        public async Task<List<Playlist>> Get()
        {
            return await _mongoDBRepository.GetAsync();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AddToPlaylist(string id, [FromBody] string movieId)
        {
            await _mongoDBRepository.AddToCollectionAsync(id, movieId, "movieIds");
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _mongoDBRepository.DeleteAsync(id);
            return NoContent();
        }
    }
}
